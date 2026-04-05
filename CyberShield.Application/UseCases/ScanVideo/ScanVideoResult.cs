namespace CyberShield.Application.UseCases.ScanVideo;

public sealed record ScanVideoResult(
    Guid ScanId,
    string FileHash,
    double OverallScore,
    string? ReportPath);