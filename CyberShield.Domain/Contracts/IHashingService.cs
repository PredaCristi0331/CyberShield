namespace CyberShield.Domain.Contracts;

public interface IHashingService
{
    Task<string> ComputeFileHashAsync(string filePath, CancellationToken ct);
}