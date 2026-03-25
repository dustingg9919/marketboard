using CoffeeDashboard.Application.Contracts;
using CoffeeDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeDashboard.Infrastructure.Services;

public class CachedDashboardService(DashboardDbContext dbContext, LiveDashboardService liveDashboardService) : IDashboardService
{
    private static readonly string[] RequiredCodes =
    [
        "COFFEE_DOMESTIC",
        "USDVND",
        "LONDON_ROBUSTA",
        "GOLD",
        "SILVER",
        "OIL",
        "VNINDEX",
        "BTCUSDT",
        "ETHUSDT",
        "BNBUSDT",
        "SOLUSDT",
        "XRPUSDT",
        "ADAUSDT"
    ];

    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var snapshots = await dbContext.MarketSnapshots
            .Include(x => x.MarketAsset)
            .Where(x => x.SnapshotDate == today)
            .ToListAsync(cancellationToken);

        var hasAllRequired = RequiredCodes.All(code => snapshots.Any(s => s.MarketAsset?.Code == code));

        if (!snapshots.Any() || !hasAllRequired)
        {
            var live = await liveDashboardService.GetSummaryAsync(cancellationToken);
            await UpsertSnapshotAsync(live.Markets, today, cancellationToken);
            await CleanupOldSnapshotsAsync(cancellationToken);
            return live;
        }

        var markets = snapshots
            .OrderBy(x => x.MarketAsset!.Code)
            .Select(s => new MarketCard
            {
                Code = s.MarketAsset!.Code,
                Label = s.MarketAsset!.Label,
                Value = s.Value,
                Unit = s.Unit,
                SecondaryValue = s.SecondaryValue,
                SecondaryUnit = s.SecondaryUnit,
                Change = s.Change,
                ChangePercent = s.ChangePercent,
                UpdatedAt = s.CreatedAt
            })
            .ToList();

        // news stays live
        var news = (await liveDashboardService.GetSummaryAsync(cancellationToken)).LatestNews;

        return new DashboardSummaryResponse
        {
            LastUpdatedAt = DateTime.UtcNow,
            Markets = markets,
            LatestNews = news
        };
    }

    private async Task UpsertSnapshotAsync(List<MarketCard> markets, DateOnly date, CancellationToken cancellationToken)
    {
        foreach (var card in markets)
        {
            var asset = await dbContext.MarketAssets
                .FirstOrDefaultAsync(x => x.Code == card.Code, cancellationToken);

            if (asset == null)
            {
                asset = new MarketAsset
                {
                    Code = card.Code,
                    Label = card.Label,
                    Unit = card.Unit,
                    Type = ResolveType(card.Code)
                };
                dbContext.MarketAssets.Add(asset);
            }
            else
            {
                asset.Label = card.Label;
                asset.Unit = card.Unit;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var snapshot = await dbContext.MarketSnapshots
                .FirstOrDefaultAsync(x => x.MarketAssetId == asset.Id && x.SnapshotDate == date, cancellationToken);

            if (snapshot == null)
            {
                snapshot = new MarketSnapshot
                {
                    MarketAssetId = asset.Id,
                    SnapshotDate = date
                };
                dbContext.MarketSnapshots.Add(snapshot);
            }

            snapshot.Value = card.Value;
            snapshot.Unit = card.Unit;
            snapshot.SecondaryValue = card.SecondaryValue;
            snapshot.SecondaryUnit = card.SecondaryUnit;
            snapshot.Change = card.Change;
            snapshot.ChangePercent = card.ChangePercent;
            snapshot.CreatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CleanupOldSnapshotsAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2));
        var oldSnapshots = await dbContext.MarketSnapshots
            .Where(x => x.SnapshotDate < cutoff)
            .ToListAsync(cancellationToken);

        if (oldSnapshots.Count > 0)
        {
            dbContext.MarketSnapshots.RemoveRange(oldSnapshots);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string ResolveType(string code)
    {
        if (code.EndsWith("USDT", StringComparison.OrdinalIgnoreCase)) return "crypto";
        if (code is "GOLD" or "SILVER") return "metal";
        if (code is "OIL") return "commodity";
        if (code is "USDVND") return "fx";
        return "market";
    }
}
