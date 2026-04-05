namespace CyberShield.Domain.Contracts;

public interface IReportService
{
    Task<string> GenerateReportAsync(Guid scanId, CancellationToken ct);
}