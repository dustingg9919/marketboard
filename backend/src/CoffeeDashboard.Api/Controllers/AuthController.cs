using CoffeeDashboard.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (result is null)
        {
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });
        }

        return Ok(result);
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            username = "admin",
            role = "Admin",
            mode = "demo"
        });
    }
}
