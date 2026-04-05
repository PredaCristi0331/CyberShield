namespace CyberShield.Domain.Models;

public sealed class VideoFrame
{
    public required TimeSpan Timestamp { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }

    
    public required byte[] PixelBytes { get; init; }
}