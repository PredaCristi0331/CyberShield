namespace CyberShield.Domain.Models;

public sealed class AudioResult
{
    public required TimeSpan Start { get; init; }
    public required TimeSpan End { get; init; }
    public required double FakeProbability { get; init; } 
}