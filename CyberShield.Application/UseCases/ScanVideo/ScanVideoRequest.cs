using CyberShield.Domain.Contracts;

namespace CyberShield.Application.UseCases.ScanVideo;

public sealed record ScanVideoRequest(
    string FilePath,
    FrameDecodeOptions DecodeOptions,
    PreprocessOptions PreprocessOptions,
    int BatchSize,
    bool AllowCache);