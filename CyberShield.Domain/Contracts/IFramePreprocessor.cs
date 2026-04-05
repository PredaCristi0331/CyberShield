using CyberShield.Domain.Models;

namespace CyberShield.Domain.Contracts;

public interface IFramePreprocessor
{
    TensorInput Preprocess(VideoFrame frame, PreprocessOptions options);
}

public sealed record PreprocessOptions(
    int TargetWidth,
    int TargetHeight,
    bool Normalize01,
    bool BgrToRgb);