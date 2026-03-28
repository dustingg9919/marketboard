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
    public DbSet<AiHookAccount> AiHookAccounts => Set<AiHookAccount>();

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

        modelBuilder.Entity<AiHookAccount>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Password).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ApiKey).HasMaxLength(500);
            entity.Property(x => x.PaymentType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.BankAccount).HasMaxLength(100);
            entity.Property(x => x.BankName).HasMaxLength(100);
        });

        modelBuilder.Entity<AiHookAccount>().HasData(new AiHookAccount
        {
            Id = Guid.Parse("3f1d4f83-0a8c-4a70-9f7c-5ecbfb1a4ad1"),
            Username = "sieu",
            Password = "sieu",
            ApiKey = null,
            PaymentType = "Dùng thử",
            ExpirationDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            ExpirationTimes = 20,
            BankAccount = null,
            BankName = null,
            CreatedAt = new DateTime(2026, 3, 28, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
