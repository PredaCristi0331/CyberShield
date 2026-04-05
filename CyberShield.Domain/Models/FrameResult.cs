namespace CyberShield.Domain.Models;

public sealed class FrameResult
{
    public required TimeSpan Timestamp { get; init; }
    public required double Score { get; init; } 
}