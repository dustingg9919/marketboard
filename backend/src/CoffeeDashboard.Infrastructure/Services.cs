using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using CoffeeDashboard.Application.Contracts;
using CoffeeDashboard.Domain.Entities;

namespace CoffeeDashboard.Infrastructure.Services;

public class DemoAuthService : IAuthService
{
    public Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) && request.Password == "admin")
        {
            return Task.FromResult<LoginResponse?>(new LoginResponse(
                AccessToken: "demo-token",
                Username: request.Username,
                Role: "Admin"));
        }

        return Task.FromResult<LoginResponse?>(null);
    }
}

public class LiveDashboardService(HttpClient httpClient) : IDashboardService
{
    private const string CoinGeckoUrl = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin,ethereum,binancecoin,solana,ripple,cardano&vs_currencies=usd&include_24hr_change=true";
    private const string VnExpressRssUrl = "https://vnexpress.net/rss/kinh-doanh.rss";
    private const string FxUrl = "https://open.er-api.com/v6/latest/USD";
    private const string GoldUrl = "https://api.gold-api.com/price/XAU";
    private const string SilverUrl = "https://api.gold-api.com/price/XAG";
    private const string OilUrl = "https://stooq.com/q/l/?s=cl.f&i=d";
    private const string VnIndexUrl = "https://cafef.vn/du-lieu/Ajax/PageNew/DataHistory/PriceHistory.ashx?Symbol=VNINDEX&StartDate=&EndDate=&PageIndex=1&PageSize=1";
    private const string GiaBacUrl = "https://giabac.net/";

    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var cryptoTask = GetCryptoCardsAsync(cancellationToken);
        var newsTask = GetLatestNewsAsync(cancellationToken);
        var fxTask = GetUsdVndCardAsync(cancellationToken);
        var domesticPreciousTask = GetDomesticPreciousPricesAsync(cancellationToken);
        var oilTask = GetOilCardAsync(cancellationToken);
        var vnIndexTask = GetVnIndexCardAsync(cancellationToken);

        var crypto = await AwaitOrFallback(cryptoTask, new List<MarketCard>(), TimeSpan.FromSeconds(4));
        var news = await AwaitOrFallback(newsTask, new List<NewsArticle>(), TimeSpan.FromSeconds(4));
        var fx = await AwaitOrFallback(fxTask, PendingCard("USDVND", "USD/VND"), TimeSpan.FromSeconds(3));
        var preciousFallback = (PendingCard("GOLD", "Giá vàng"), PendingCard("SILVER", "Giá bạc"));
        var precious = await AwaitOrFallback(domesticPreciousTask, preciousFallback, TimeSpan.FromSeconds(4));
        var oil = await AwaitOrFallback(oilTask, PendingCard("OIL", "Giá dầu"), TimeSpan.FromSeconds(3));
        var vnindex = await AwaitOrFallback(vnIndexTask, PendingCard("VNINDEX", "VN-Index"), TimeSpan.FromSeconds(3));

        var markets = new List<MarketCard>
        {
            new() { Code = "COFFEE_DOMESTIC", Label = "Cà phê nội địa", Value = 0, Unit = string.Empty, Change = null, ChangePercent = null },
            fx,
            new() { Code = "LONDON_ROBUSTA", Label = "London Robusta", Value = 0, Unit = string.Empty, Change = null, ChangePercent = null },
            precious.Gold,
            precious.Silver,
            oil,
            vnindex
        };

        markets.AddRange(crypto);

        return new DashboardSummaryResponse
        {
            LastUpdatedAt = DateTime.UtcNow,
            Markets = markets,
            LatestNews = news
        };
    }

    private async Task<List<MarketCard>> GetCryptoCardsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(CoinGeckoUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<Dictionary<string, CryptoPriceDto>>(stream, cancellationToken: cancellationToken)
                          ?? new Dictionary<string, CryptoPriceDto>();

            var map = new (string Id, string Code, string Label)[]
            {
                ("bitcoin", "BTCUSDT", "Bitcoin"),
                ("ethereum", "ETHUSDT", "Ethereum"),
                ("binancecoin", "BNBUSDT", "BNB"),
                ("solana", "SOLUSDT", "Solana"),
                ("ripple", "XRPUSDT", "XRP"),
                ("cardano", "ADAUSDT", "Cardano")
            };

            return map
                .Where(x => payload.ContainsKey(x.Id))
                .Select(x => new MarketCard
                {
                    Code = x.Code,
                    Label = x.Label,
                    Value = payload[x.Id].Usd,
                    Unit = "USD",
                    ChangePercent = Math.Round(payload[x.Id].Usd24hChange ?? 0, 2),
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();
        }
        catch
        {
            return
            [
                new() { Code = "BTCUSDT", Label = "Bitcoin", Value = 68357, Unit = "USD", ChangePercent = -0.60m, UpdatedAt = DateTime.UtcNow },
                new() { Code = "ETHUSDT", Label = "Ethereum", Value = 2045.48m, Unit = "USD", ChangePercent = -1.96m, UpdatedAt = DateTime.UtcNow },
                new() { Code = "BNBUSDT", Label = "BNB", Value = 624.16m, Unit = "USD", ChangePercent = -1.08m, UpdatedAt = DateTime.UtcNow },
                new() { Code = "SOLUSDT", Label = "Solana", Value = 86.03m, Unit = "USD", ChangePercent = -2.01m, UpdatedAt = DateTime.UtcNow },
                new() { Code = "XRPUSDT", Label = "XRP", Value = 1.37m, Unit = "USD", ChangePercent = -2.27m, UpdatedAt = DateTime.UtcNow },
                new() { Code = "ADAUSDT", Label = "Cardano", Value = 0.250072m, Unit = "USD", ChangePercent = -2.75m, UpdatedAt = DateTime.UtcNow }
            ];
        }
    }

    private async Task<MarketCard> GetUsdVndCardAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(FxUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<FxApiResponse>(stream, cancellationToken: cancellationToken);
            var vnd = payload?.Rates?.GetValueOrDefault("VND");

            if (vnd is > 0)
            {
                return new MarketCard
                {
                    Code = "USDVND",
                    Label = "USD/VND",
                    Value = Math.Round(vnd.Value, 2),
                    Unit = "VND",
                    ChangePercent = null,
                    UpdatedAt = DateTime.UtcNow
                };
            }
        }
        catch
        {
        }

        return PendingCard("USDVND", "USD/VND");
    }

    private async Task<(MarketCard Gold, MarketCard Silver)> GetDomesticPreciousPricesAsync(CancellationToken cancellationToken)
    {
        var goldUsdTask = GetUsdMetalPriceAsync(GoldUrl, cancellationToken);
        var silverUsdTask = GetUsdMetalPriceAsync(SilverUrl, cancellationToken);
        var domesticTask = GetGiaBacDomesticAsync(cancellationToken);

        await Task.WhenAll(goldUsdTask, silverUsdTask, domesticTask);

        var domestic = await domesticTask;
        var goldUsd = await goldUsdTask;
        var silverUsd = await silverUsdTask;

        var gold = domestic.GoldVndPerChi is > 0
            ? new MarketCard
            {
                Code = "GOLD",
                Label = "Giá vàng",
                Value = domestic.GoldVndPerChi.Value,
                Unit = "VND/chỉ",
                SecondaryValue = goldUsd,
                SecondaryUnit = goldUsd is > 0 ? "USD/oz" : null,
                UpdatedAt = DateTime.UtcNow
            }
            : PendingCard("GOLD", "Giá vàng");

        var silver = domestic.SilverVndPerLuong is > 0
            ? new MarketCard
            {
                Code = "SILVER",
                Label = "Giá bạc",
                Value = domestic.SilverVndPerLuong.Value,
                Unit = "VND/lượng",
                SecondaryValue = silverUsd,
                SecondaryUnit = silverUsd is > 0 ? "USD/oz" : null,
                UpdatedAt = DateTime.UtcNow
            }
            : PendingCard("SILVER", "Giá bạc");

        return (gold, silver);
    }

    private async Task<decimal?> GetUsdMetalPriceAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<MetalApiResponse>(stream, cancellationToken: cancellationToken);
            if (payload?.Price is > 0)
            {
                return Math.Round(payload.Price.Value, 2);
            }
        }
        catch
        {
        }

        return null;
    }

    private async Task<(decimal? GoldVndPerChi, decimal? SilverVndPerLuong)> GetGiaBacDomesticAsync(CancellationToken cancellationToken)
    {
        try
        {
            var html = await httpClient.GetStringAsync(GiaBacUrl, cancellationToken);

            decimal? silver = null;
            decimal? gold = null;

            var silverMatch = System.Text.RegularExpressions.Regex.Match(
                html,
                @"1 Lu.ng</td><td align=""right"">([\d,]+)</td><td align=""right"">([\d,]+)</td>",
                System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (silverMatch.Success)
            {
                silver = ParseDecimal(silverMatch.Groups[2].Value);
            }

            var goldMatch = System.Text.RegularExpressions.Regex.Match(
                html,
                @"<tr><td>Gold</td><td>[\d,.]+</td><td>[\d,]+</td><td>([\d,]+)</td></tr>",
                System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (goldMatch.Success)
            {
                gold = ParseDecimal(goldMatch.Groups[1].Value);
            }

            return (gold, silver);
        }
        catch
        {
            return (null, null);
        }
    }

    private async Task<MarketCard> GetOilCardAsync(CancellationToken cancellationToken)
    {
        try
        {
            var csv = await httpClient.GetStringAsync(OilUrl, cancellationToken);
            var line = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(line))
            {
                var parts = line.Split(',');
                if (parts.Length >= 7 && decimal.TryParse(parts[6], CultureInfo.InvariantCulture, out var closePrice))
                {
                    return new MarketCard
                    {
                        Code = "OIL",
                        Label = "Giá dầu",
                        Value = closePrice,
                        Unit = "USD/thùng",
                        ChangePercent = null,
                        UpdatedAt = DateTime.UtcNow
                    };
                }
            }
        }
        catch
        {
        }

        return PendingCard("OIL", "Giá dầu");
    }

    private async Task<MarketCard> GetVnIndexCardAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(VnIndexUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<VnIndexApiResponse>(stream, cancellationToken: cancellationToken);
            var latest = payload?.Data?.Data?.FirstOrDefault();

            if (latest is not null && latest.GiaDongCua > 0)
            {
                return new MarketCard
                {
                    Code = "VNINDEX",
                    Label = "VN-Index",
                    Value = latest.GiaDongCua,
                    Unit = "điểm",
                    ChangePercent = ParsePercent(latest.ThayDoi),
                    UpdatedAt = DateTime.UtcNow
                };
            }
        }
        catch
        {
        }

        return PendingCard("VNINDEX", "VN-Index");
    }

    private async Task<List<NewsArticle>> GetLatestNewsAsync(CancellationToken cancellationToken)
    {
        var xml = await httpClient.GetStringAsync(VnExpressRssUrl, cancellationToken);
        var document = XDocument.Parse(xml);

        return document.Descendants("item")
            .Take(5)
            .Select(item => new NewsArticle
            {
                Category = "Kinh doanh",
                Title = item.Element("title")?.Value?.Trim() ?? string.Empty,
                Summary = ExtractSummary(item.Element("description")?.Value),
                Url = item.Element("link")?.Value?.Trim() ?? "https://vnexpress.net",
                PublishedAt = ParseRssDate(item.Element("pubDate")?.Value)
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .ToList();
    }

    private static async Task<T> AwaitOrFallback<T>(Task<T> task, T fallback, TimeSpan timeout)
    {
        var completed = await Task.WhenAny(task, Task.Delay(timeout));
        if (completed == task)
        {
            return await task;
        }

        return fallback;
    }

    private static MarketCard PendingCard(string code, string label) => new()
    {
        Code = code,
        Label = label,
        Value = 0,
        Unit = string.Empty,
        ChangePercent = null,
        UpdatedAt = DateTime.UtcNow
    };

    private static string ExtractSummary(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var clean = description
            .Replace("<![CDATA[", string.Empty)
            .Replace("]]>", string.Empty);

        var brIndex = clean.LastIndexOf("</br>", StringComparison.OrdinalIgnoreCase);
        if (brIndex >= 0)
        {
            clean = clean[(brIndex + 5)..];
        }

        return System.Text.RegularExpressions.Regex.Replace(clean, "<.*?>", string.Empty).Trim();
    }

    private static DateTime ParseRssDate(string? value)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }

    private static decimal? ParsePercent(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var start = value.IndexOf('(');
        var end = value.IndexOf('%');
        if (start >= 0 && end > start)
        {
            var percentText = value[(start + 1)..end]
                .Replace(',', '.')
                .Trim();

            if (decimal.TryParse(percentText, NumberStyles.Any, CultureInfo.InvariantCulture, out var percent))
            {
                return percent;
            }
        }

        return null;
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Replace(",", string.Empty).Trim();
        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private sealed class CryptoPriceDto
    {
        [JsonPropertyName("usd")]
        public decimal Usd { get; set; }

        [JsonPropertyName("usd_24h_change")]
        public decimal? Usd24hChange { get; set; }
    }

    private sealed class FxApiResponse
    {
        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }

    private sealed class MetalApiResponse
    {
        [JsonPropertyName("price")]
        public decimal? Price { get; set; }
    }

    private sealed class VnIndexApiResponse
    {
        [JsonPropertyName("Data")]
        public VnIndexDataWrapper? Data { get; set; }
    }

    private sealed class VnIndexDataWrapper
    {
        [JsonPropertyName("Data")]
        public List<VnIndexItem>? Data { get; set; }
    }

    private sealed class VnIndexItem
    {
        [JsonPropertyName("GiaDongCua")]
        public decimal GiaDongCua { get; set; }

        [JsonPropertyName("ThayDoi")]
        public string? ThayDoi { get; set; }
    }
}
