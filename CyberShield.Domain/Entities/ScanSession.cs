namespace CyberShield.Domain.Entities;

public sealed class ScanSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FilePath { get; init; } = "";
    public string FileHash { get; init; } = "";
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAt { get; set; }

    public string ModelVersion { get; set; } = "unknown";
    public double OverallScore { get; set; } 
    public string Status { get; set; } = "Running";

    public List<ScanSegment> Segments { get; } = [];
}