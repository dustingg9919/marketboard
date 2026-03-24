using CoffeeDashboard.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketsController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var summary = await dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(summary.Markets);
    }
}
