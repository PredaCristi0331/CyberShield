using CyberShield.Domain.Models;

namespace CyberShield.Domain.Contracts;

public interface IAudioExtractor
{
    
    IAsyncEnumerable<byte[]> ExtractAudioChunksAsync(string filePath, CancellationToken ct);
}