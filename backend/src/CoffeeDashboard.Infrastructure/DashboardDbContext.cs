using CoffeeDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeDashboard.Infrastructure;

public class DashboardDbContext : DbContext
{
    public DashboardDbContext(DbContextOptions<DashboardDbContext> options) : base(options)
    {
    }

    public DbSet<MarketAsset> MarketAssets => Set<MarketAsset>();
    public DbSet<MarketSnapshot> MarketSnapshots => Set<MarketSnapshot>();
    public DbSet<ApiAccountRecord> ApiAccounts => Set<ApiAccountRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MarketAsset>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Unit).HasMaxLength(50);
            entity.Property(x => x.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<MarketSnapshot>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.MarketAssetId, x.SnapshotDate }).IsUnique();
            entity.Property(x => x.Unit).HasMaxLength(50);
            entity.Property(x => x.SecondaryUnit).HasMaxLength(50);
            entity.Property(x => x.SnapshotDate).HasColumnType("date");
            entity.HasOne(x => x.MarketAsset)
                .WithMany()
                .HasForeignKey(x => x.MarketAssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiAccountRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(100);
        });
    }
}
