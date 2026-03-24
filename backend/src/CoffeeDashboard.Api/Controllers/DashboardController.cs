using CoffeeDashboard.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("last-updated")]
    public IActionResult GetLastUpdated()
    {
        return Ok(new { lastUpdatedAt = DateTime.UtcNow });
    }
}
