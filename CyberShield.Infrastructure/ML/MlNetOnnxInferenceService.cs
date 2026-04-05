using CyberShield.Domain.Contracts;
using CyberShield.Domain.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CyberShield.Infrastructure.ML;

public sealed class MlNetOnnxInferenceService : IInferenceService
{
    private readonly MLContext _ml = new(seed: 1);
    private readonly ITransformer _model;

   
    private readonly string _inputColumnName;
    private readonly string _outputColumnName;

    public string ModelVersion { get; }

    public MlNetOnnxInferenceService(
        string modelPath,
        string modelVersion,
        string inputColumnName = "input",
        string outputColumnName = "score")
    {
        ModelVersion = modelVersion;
        _inputColumnName = inputColumnName;
        _outputColumnName = outputColumnName;

        
        var emptyData = _ml.Data.LoadFromEnumerable(Array.Empty<ModelInput>());

        var pipeline = _ml.Transforms.ApplyOnnxModel(
            modelFile: modelPath,
            outputColumnNames: new[] { _outputColumnName },
            inputColumnNames: new[] { _inputColumnName });

        _model = pipeline.Fit(emptyData);
    }

    public Task<IReadOnlyList<FrameResult>> RunBatchAsync(
        IReadOnlyList<TensorInput> batch,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        
        var inputs = batch.Select(t => new ModelInput
        {
            TimestampMs = (long)t.Timestamp.TotalMilliseconds,
            Input = t.Data
        });

        
        var dv = _ml.Data.LoadFromEnumerable(inputs);
        var transformed = _model.Transform(dv);

        
        var preds = _ml.Data.CreateEnumerable<ModelOutput>(transformed, reuseRowObject: false).ToArray();

        
        var results = preds.Select(p => new FrameResult
        {
            Timestamp = TimeSpan.FromMilliseconds(p.TimestampMs),
            Score = Clamp01(p.Score)
        }).ToArray();

        return Task.FromResult<IReadOnlyList<FrameResult>>(results);
    }

    private static double Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;

    private sealed class ModelInput
    {
        public long TimestampMs { get; set; }

        
        [ColumnName("input")]
        [VectorType] 
        public float[] Input { get; set; } = Array.Empty<float>();
    }

    private sealed class ModelOutput
    {
        public long TimestampMs { get; set; }

        
        [ColumnName("score")]
        public float Score { get; set; }
    }
}