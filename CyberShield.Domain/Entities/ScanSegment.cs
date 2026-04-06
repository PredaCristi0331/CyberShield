using System;

namespace CyberShield.Domain.Entities; 

public class ScanSegment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScanSessionId { get; set; }


    public TimeSpan Start { get; init; }
    public TimeSpan End { get; init; }
    public double Score { get; init; }
    public string Notes { get; set; } = string.Empty;
}