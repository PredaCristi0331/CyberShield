using CyberShield.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CyberShield.Infrastructure.Reporting;

public sealed class QuestPdfReportService : IReportService
{
    private readonly Persistence.CyberShieldDbContext _db;

    public QuestPdfReportService(Persistence.CyberShieldDbContext db)
    {
        _db = db;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateReportAsync(Guid scanId, CancellationToken ct)
    {
        var scan = await _db.Scans.AsNoTracking().FirstAsync(x => x.Id == scanId, ct);

        var reportPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "CyberShield",
            "Reports",
            $"{scan.Id}.pdf");

        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.Header().Text("CyberShield Deepfake Detector Report").SemiBold().FontSize(18);

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text($"Scan ID: {scan.Id}");
                    col.Item().Text($"File: {scan.FilePath}");
                    col.Item().Text($"Hash: {scan.FileHash}");
                    col.Item().Text($"Model: {scan.ModelVersion}");
                    col.Item().Text($"Overall score: {scan.OverallScore:0.000}");
                    col.Item().Text($"Started: {scan.StartedAt:u}");
                    col.Item().Text($"Finished: {scan.FinishedAt:u}");
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated at ");
                    x.Span(DateTimeOffset.UtcNow.ToString("u"));
                });
            });
        }).GeneratePdf(reportPath);

        return reportPath;
    }
}