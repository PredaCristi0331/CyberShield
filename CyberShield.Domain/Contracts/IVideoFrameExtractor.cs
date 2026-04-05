using CyberShield.Domain.Models;

namespace CyberShield.Domain.Contracts;

public interface IVideoFrameExtractor
{
    IAsyncEnumerable<VideoFrame> DecodeFramesAsync(
        string filePath,
        FrameDecodeOptions options,
        CancellationToken cancellationToken);
}

public sealed record FrameDecodeOptions(
    double TargetFps,
    int? MaxFrames,
    bool UseHardwareDecode);