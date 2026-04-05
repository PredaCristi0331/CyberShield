using System.Security.Cryptography;
using CyberShield.Domain.Contracts;

namespace CyberShield.Infrastructure.Security;

public sealed class Sha256HashingService : IHashingService
{
    public async Task<string> ComputeFileHashAsync(string filePath, CancellationToken ct)
    {
        await using var stream = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(hash);
    }
}