using System;

namespace CyberShield.Infrastructure.Persistence.Entities;

public sealed class ScanSegmentEntity
{
    public Guid Id { get; set; }
    public Guid ScanSessionId { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public double Score { get; set; }

    public double AverageScore {  get; set; }
}