using System;

namespace CyberShield.Domain.Entities; 

public class ScanSegment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScanSessionId { get; set; }

    
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public double Score { get; set; }
    public string Notes { get; set; } = string.Empty;
}