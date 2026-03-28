using CoffeeDashboard.Domain.Entities;
using CoffeeDashboard.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/ai-hook")]
public class AiHookController(DashboardDbContext dbContext) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AiHookLoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var password = request.Password.Trim();

        var account = await dbContext.AiHookAccounts
            .FirstOrDefaultAsync(x => x.Username == username && x.Password == password, cancellationToken);

        if (account == null)
        {
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });
        }

        return Ok(ToDto(account));
    }

    [HttpPost("save-key")]
    public async Task<IActionResult> SaveKey([FromBody] AiHookSaveKeyRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var account = await dbContext.AiHookAccounts
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (account == null)
        {
            return NotFound(new { message = "Account not found" });
        }

        account.ApiKey = request.ApiKey?.Trim();
        account.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(account));
    }

    [HttpPost("consume")]
    public async Task<IActionResult> Consume([FromBody] AiHookConsumeRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var account = await dbContext.AiHookAccounts
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (account == null)
        {
            return NotFound(new { message = "Account not found" });
        }

        var now = DateTime.UtcNow.Date;
        if (account.ExpirationDate.Date < now || account.ExpirationTimes <= 0)
        {
            return Ok(new { expired = true, message = "Tài khoản của bạn đã hết hạn" });
        }

        account.ExpirationTimes = Math.Max(0, account.ExpirationTimes - 1);
        account.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { expired = false, remaining = account.ExpirationTimes });
    }

    private static AiHookAccountDto ToDto(AiHookAccount account)
    {
        return new AiHookAccountDto(
            account.Username,
            account.ApiKey,
            account.PaymentType,
            account.ExpirationDate,
            account.ExpirationTimes,
            account.BankAccount,
            account.BankName
        );
    }
}

public record AiHookLoginRequest(string Username, string Password);
public record AiHookSaveKeyRequest(string Username, string? ApiKey);
public record AiHookConsumeRequest(string Username);

public record AiHookAccountDto(
    string Username,
    string? ApiKey,
    string PaymentType,
    DateTime ExpirationDate,
    int ExpirationTimes,
    string? BankAccount,
    string? BankName
);
