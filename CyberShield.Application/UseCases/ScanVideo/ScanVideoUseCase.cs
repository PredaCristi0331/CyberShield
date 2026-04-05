using System.Threading.Channels;
using CyberShield.Domain.Contracts;
using CyberShield.Domain.Entities;
using CyberShield.Domain.Models;
using CyberShield.Shared.Progress;
using Microsoft.Extensions.Logging;

namespace CyberShield.Application.UseCases.ScanVideo;

public sealed class ScanVideoUseCase
{
    private readonly IVideoFrameExtractor _extractor;
    private readonly IFramePreprocessor _preprocessor;
    private readonly IInferenceService _inference;
    private readonly IScanRepository _repo;
    private readonly IHashingService _hashing;
    private readonly IReportService _report;
    private readonly ILogger<ScanVideoUseCase> _log;

    public ScanVideoUseCase(
        IVideoFrameExtractor extractor,
        IFramePreprocessor preprocessor,
        IInferenceService inference,
        IScanRepository repo,
        IHashingService hashing,
        IReportService report,
        ILogger<ScanVideoUseCase> log)
    {
        _extractor = extractor;
        _preprocessor = preprocessor;
        _inference = inference;
        _repo = repo;
        _hashing = hashing;
        _report = report;
        _log = log;
    }

    public async Task<ScanVideoResult> ExecuteAsync(
        ScanVideoRequest request,
        IProgress<ScanProgress>? progress,
        CancellationToken ct)
    {
        progress?.Report(new ScanProgress("Hashing", 0, "Computing file hash..."));
        var fileHash = await _hashing.ComputeFileHashAsync(request.FilePath, ct);

        if (request.AllowCache)
        {
            var cached = await _repo.FindByFileHashAsync(fileHash, ct);
            if (cached is not null && cached.FinishedAt is not null)
            {
                progress?.Report(new ScanProgress("Cache", 100, "Loaded cached result."));
                return new ScanVideoResult(cached.Id, fileHash, cached.OverallScore, ReportPath: null);
            }
        }

        var scan = new ScanSession
        {
            FilePath = request.FilePath,
            FileHash = fileHash,
            ModelVersion = _inference.ModelVersion,
            Status = "Running"
        };

        await _repo.CreateScanSessionAsync(scan, ct);

        
        var frameChannel = Channel.CreateBounded<VideoFrame>(new BoundedChannelOptions(64)
        {
            SingleWriter = true,
            SingleReader = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        var tensorChannel = Channel.CreateBounded<TensorInput>(new BoundedChannelOptions(64)
        {
            SingleWriter = false,
            SingleReader = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        var resultChannel = Channel.CreateBounded<FrameResult>(new BoundedChannelOptions(256)
        {
            SingleWriter = false,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var aggregator = new ResultsAggregator();

        
        var decodeTask = Task.Run(async () =>
        {
            try
            {
                progress?.Report(new ScanProgress("Decode", 0, "Decoding frames..."));

                await foreach (var frame in _extractor.DecodeFramesAsync(request.FilePath, request.DecodeOptions, ct))
                {
                    await frameChannel.Writer.WriteAsync(frame, ct);
                }
            }
            finally
            {
                frameChannel.Writer.TryComplete();
            }
        }, ct);

        
        var preprocessWorkers = Enumerable.Range(0, Environment.ProcessorCount / 2 + 1)
            .Select(_ => Task.Run(async () =>
            {
                try
                {
                    await foreach (var frame in frameChannel.Reader.ReadAllAsync(ct))
                    {
                        var tensor = _preprocessor.Preprocess(frame, request.PreprocessOptions);
                        await tensorChannel.Writer.WriteAsync(tensor, ct);
                    }
                }
                catch (OperationCanceledException) { }
            }, ct))
            .ToArray();

        
        var preprocessCloseTask = Task.Run(async () =>
        {
            try { await Task.WhenAll(preprocessWorkers); }
            finally { tensorChannel.Writer.TryComplete(); }
        }, ct);

        
        var inferenceTask = Task.Run(async () =>
        {
            var batch = new List<TensorInput>(request.BatchSize);

            try
            {
                progress?.Report(new ScanProgress("Inference", 0, "Running AI inference..."));

                await foreach (var item in tensorChannel.Reader.ReadAllAsync(ct))
                {
                    batch.Add(item);
                    if (batch.Count >= request.BatchSize)
                    {
                        await RunBatchAndPublishAsync(batch, resultChannel.Writer, ct);
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    await RunBatchAndPublishAsync(batch, resultChannel.Writer, ct);
                    batch.Clear();
                }
            }
            finally
            {
                resultChannel.Writer.TryComplete();
            }
        }, ct);

        
        var consumeTask = Task.Run(async () =>
        {
            var buffer = new List<FrameResult>(64);

            await foreach (var r in resultChannel.Reader.ReadAllAsync(ct))
            {
                buffer.Add(r);
                if (buffer.Count >= 64)
                {
                    aggregator.AddResults(scan.Id, buffer);
                    await _repo.AppendFrameResultsAsync(scan.Id, buffer, ct);
                    buffer.Clear();

                    progress?.Report(new ScanProgress("Persist", 0, "Persisted partial results..."));
                }
            }

            if (buffer.Count > 0)
            {
                aggregator.AddResults(scan.Id, buffer);
                await _repo.AppendFrameResultsAsync(scan.Id, buffer, ct);
            }
        }, ct);

        
        await Task.WhenAll(decodeTask, preprocessCloseTask, inferenceTask, consumeTask);

        
        var overall = aggregator.ComputeOverallScore();
        var segments = aggregator.BuildSegments(scan.Id);

        await _repo.FinalizeScanAsync(scan.Id, overall, segments, ct);

        progress?.Report(new ScanProgress("Report", 0, "Generating PDF report..."));
        var reportPath = await _report.GenerateReportAsync(scan.Id, ct);

        progress?.Report(new ScanProgress("Done", 100, "Scan completed."));
        return new ScanVideoResult(scan.Id, fileHash, overall, reportPath);
    }

    private async Task RunBatchAndPublishAsync(
        IReadOnlyList<TensorInput> batch,
        ChannelWriter<FrameResult> writer,
        CancellationToken ct)
    {
        var results = await _inference.RunBatchAsync(batch, ct);
        foreach (var r in results)
            await writer.WriteAsync(r, ct);
    }
}