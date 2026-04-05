namespace CyberShield.Infrastructure.Persistence.Entities;

public sealed class FrameFindingEntity
{
    public Guid Id { get; set; }
    public Guid ScanSessionId { get; set; }
    public long TimestampMs { get; set; }
    public double Score { get; set; }
}