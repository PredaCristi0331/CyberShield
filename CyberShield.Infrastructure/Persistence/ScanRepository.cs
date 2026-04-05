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

    public async Task FinalizeScanAsync(Guid scanId, double overallScore, IReadOnlyList<ScanSegment> segments, CancellationToken ct)
    {
        var scan = await _db.Scans.FirstAsync(x => x.Id == scanId, ct);

        scan.OverallScore = overallScore;
        scan.FinishedAt = DateTimeOffset.UtcNow;
        scan.Status = "Completed";

        var existing = _db.Segments.Where(x => x.ScanSessionId == scanId);
        _db.Segments.RemoveRange(existing);

        foreach (var s in segments)
        {
            _db.Segments.Add(new ScanSegmentEntity
            {
                Id = s.Id,
                ScanSessionId = scanId,
                StartMs = (long)s.Start.TotalMilliseconds,
                EndMs = (long)s.End.TotalMilliseconds,
                Score = s.Score,
                Notes = s.Notes
            });
        }

        await _db.SaveChangesAsync(ct);
    }
}