using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CyberShield.Domain.Entities;
using CyberShield.Domain.Models;

namespace CyberShield.Domain.Contracts;

public interface IScanRepository
{
    Task<ScanSession?> FindByFileHashAsync(string fileHash, CancellationToken ct);

    Task CreateScanSessionAsync(ScanSession session, CancellationToken ct);

    Task AppendFrameResultsAsync(Guid scanId, IReadOnlyList<FrameResult> results, CancellationToken ct);

    
    Task FinalizeScanAsync(
        Guid scanId,
        double overallScore,
        IReadOnlyList<ScanSegment> segments,
        string? reportPath,
        CancellationToken ct);
}