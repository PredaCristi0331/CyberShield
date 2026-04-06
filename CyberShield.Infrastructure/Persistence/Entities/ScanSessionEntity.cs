using System;
using System.Collections.Generic;

namespace CyberShield.Infrastructure.Persistence.Entities;

public sealed class ScanSessionEntity
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = "";
    public string FileHash { get; set; } = "";
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string ModelVersion { get; set; } = "";
    public double OverallScore { get; set; }
    public string Status { get; set; } = "";
    public string? ReportPath { get; set; }

    
    public List<ScanSegmentEntity> Segments { get; set; } = new();
}