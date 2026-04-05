using CyberShield.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.Infrastructure.Persistence;

public sealed class CyberShieldDbContext : DbContext
{
    public CyberShieldDbContext(DbContextOptions<CyberShieldDbContext> options) : base(options) { }

    public DbSet<ScanSessionEntity> Scans => Set<ScanSessionEntity>();
    public DbSet<FrameFindingEntity> FrameFindings => Set<FrameFindingEntity>();
    public DbSet<ScanSegmentEntity> Segments => Set<ScanSegmentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScanSessionEntity>().HasKey(x => x.Id);
        modelBuilder.Entity<ScanSessionEntity>().HasIndex(x => x.FileHash);

        modelBuilder.Entity<FrameFindingEntity>().HasKey(x => x.Id);
        modelBuilder.Entity<FrameFindingEntity>().HasIndex(x => new { x.ScanSessionId, x.TimestampMs });

        modelBuilder.Entity<ScanSegmentEntity>().HasKey(x => x.Id);
        modelBuilder.Entity<ScanSegmentEntity>().HasIndex(x => x.ScanSessionId);
    }
}