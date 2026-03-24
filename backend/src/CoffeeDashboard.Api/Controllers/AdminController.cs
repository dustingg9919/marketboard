using Microsoft.AspNetCore.Mvc;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    [HttpGet("jobs")]
    public IActionResult GetJobs()
    {
        return Ok(new[]
        {
            new { jobName = "CryptoMarketJob", status = "Pending" },
            new { jobName = "VnExpressNewsJob", status = "Pending" },
            new { jobName = "CoffeeDomesticJob", status = "Pending" }
        });
    }
}
