namespace CyberShield.Domain.Models;

public sealed class TensorInput
{
    public required TimeSpan Timestamp { get; init; }

    
    public required float[] Data { get; init; }

    
    public required int[] Shape { get; init; }
}