using CyberShield.Domain.Contracts;
using CyberShield.Domain.Entities;
using CyberShield.Domain.Models;
using CyberShield.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.Infrastructure.Persistence;

public sealed class ScanRepository : IScanRepository
{
    private readonly CyberShieldDbContext _db;

    public ScanRepository(CyberShieldDbContext db) => _db = db;

    public async Task<ScanSession?> FindByFileHashAsync(string fileHash, CancellationToken ct)
    {
        var e = await _db.Scans.AsNoTracking()
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(x => x.FileHash == fileHash, ct);

        return e is null ? null : new ScanSession
        {
            Id = e.Id,
            FilePath = e.FilePath,
            FileHash = e.FileHash,
            StartedAt = e.StartedAt,
            FinishedAt = e.FinishedAt,
            ModelVersion = e.ModelVersion,
            OverallScore = e.OverallScore,
            Status = e.Status
        };
    }

    public async Task CreateScanSessionAsync(ScanSession session, CancellationToken ct)
    {
        _db.Scans.Add(new ScanSessionEntity
        {
            Id = session.Id,
            FilePath = session.FilePath,
            FileHash = session.FileHash,
            StartedAt = session.StartedAt,
            FinishedAt = session.FinishedAt,
            ModelVersion = session.ModelVersion,
            OverallScore = session.OverallScore,
            Status = session.Status
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task AppendFrameResultsAsync(Guid scanId, IReadOnlyList<FrameResult> results, CancellationToken ct)
    {
        foreach (var r in results)
        {
            _db.FrameFindings.Add(new FrameFindingEntity
            {
                Id = Guid.NewGuid(),
                ScanSessionId = scanId,
                TimestampMs = (long)r.Timestamp.TotalMilliseconds,
                Score = r.Score
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task FinalizeScanAsync(
    Guid scanId,
    double overallScore,
    IReadOnlyList<ScanSegment> segments,
    string? reportPath,
    CancellationToken ct)
    {
        
        var scan = await _db.Scans
            .Include(s => s.Segments)
            .FirstOrDefaultAsync(x => x.Id == scanId, ct);

        if (scan is null) return;

        scan.OverallScore = overallScore;
        scan.FinishedAt = DateTimeOffset.UtcNow;
        scan.Status = "Completed";
        scan.ReportPath = reportPath;

        
        scan.Segments.Clear();

        
        foreach (var seg in segments)
        {
            scan.Segments.Add(new ScanSegmentEntity
            {
                Id = Guid.NewGuid(),
                ScanSessionId = scanId,
                Start = seg.Start,
                End = seg.End,
                Score = seg.Score 
            });
        }

        await _db.SaveChangesAsync(ct);
    }
}