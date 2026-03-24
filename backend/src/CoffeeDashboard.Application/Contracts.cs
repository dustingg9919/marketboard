using CoffeeDashboard.Domain.Entities;

namespace CoffeeDashboard.Application.Contracts;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string AccessToken, string Username, string Role);

public class DashboardSummaryResponse
{
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public List<MarketCard> Markets { get; set; } = new();
    public List<NewsArticle> LatestNews { get; set; } = new();
}

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
}
