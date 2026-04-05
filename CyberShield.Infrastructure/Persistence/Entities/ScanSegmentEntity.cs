namespace CyberShield.Infrastructure.Persistence.Entities;

public sealed class ScanSegmentEntity
{
    public Guid Id { get; set; }
    public Guid ScanSessionId { get; set; }
    public long StartMs { get; set; }
    public long EndMs { get; set; }
    public double Score { get; set; }
    public string Notes { get; set; } = "";
}