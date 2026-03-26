using CoffeeDashboard.Domain.Entities;
using CoffeeDashboard.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/market")]
public class MarketDataController(DashboardDbContext dbContext) : ControllerBase
{
    [HttpPost("coffee")]
    public async Task<IActionResult> UpsertCoffeePrices([FromBody] List<CoffeePriceItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return BadRequest(new { message = "No items" });
        }

        var asset = await dbContext.MarketAssets.FirstOrDefaultAsync(x => x.Code == "COFFEE_DOMESTIC", cancellationToken);
        if (asset == null)
        {
            asset = new MarketAsset
            {
                Code = "COFFEE_DOMESTIC",
                Label = "Cà phê nội địa",
                Unit = "VND/kg",
                Type = "commodity"
            };
            dbContext.MarketAssets.Add(asset);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var item in items)
        {
            var date = DateOnly.Parse(item.Date);
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

            snapshot.Value = item.Price;
            snapshot.Unit = "VND/kg";
            snapshot.Change = item.Change;
            snapshot.ChangePercent = null;
            snapshot.CreatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { status = "ok", count = items.Count });
    }
}

public record CoffeePriceItem(string Date, decimal Price, decimal Change);
