using CoffeeDashboard.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest(CancellationToken cancellationToken)
    {
        var summary = await dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(summary.LatestNews);
    }
}
