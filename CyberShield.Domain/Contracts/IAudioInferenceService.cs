namespace CyberShield.Domain.Contracts;

public interface IAudioInferenceService
{
    
    Task<double> AnalyzeAudioChunkAsync(byte[] audioPcmBytes, CancellationToken ct);
}