using System.Net.Http.Json;
using System.Text.Json;
using CoffeeDashboard.Domain.Entities;
using CoffeeDashboard.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/ai-hook")]
public class AiHookController(DashboardDbContext dbContext, IHttpClientFactory httpClientFactory) : ControllerBase
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

    [HttpPost("describe-video")]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> DescribeVideo([FromForm] AiHookVideoDescribeRequest request, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(account.ApiKey))
        {
            return BadRequest(new { message = "Vui lòng nhập Gemini API Key trước." });
        }

        if (request.Video == null || request.Video.Length == 0)
        {
            return BadRequest(new { message = "Thiếu video sản phẩm." });
        }

        if (request.Video.Length > 50_000_000)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new { message = "Video quá lớn (tối đa 50MB)." });
        }

        await using var videoStream = request.Video.OpenReadStream();
        await using var memoryStream = new MemoryStream();
        await videoStream.CopyToAsync(memoryStream, cancellationToken);
        var base64Video = Convert.ToBase64String(memoryStream.ToArray());

        var prompt = string.IsNullOrWhiteSpace(request.Prompt)
            ? "Hãy mô tả sản phẩm trong video theo phong cách thương mại điện tử, ngắn gọn, rõ ràng, nêu điểm nổi bật, chất liệu, công dụng, và phù hợp để đăng bán."
            : request.Prompt.Trim();

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = request.Video.ContentType ?? "video/mp4",
                                data = base64Video
                            }
                        }
                    }
                }
            }
        };

        var httpClient = httpClientFactory.CreateClient("Gemini");
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={Uri.EscapeDataString(account.ApiKey)}";

        using var response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return StatusCode((int)response.StatusCode, new { message = "Gọi Gemini thất bại.", details = errorBody });
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        var text = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")
            .EnumerateArray()
            .Select(part => part.TryGetProperty("text", out var partText) ? partText.GetString() : null)
            .Where(partText => !string.IsNullOrWhiteSpace(partText))
            .DefaultIfEmpty(string.Empty)
            .Aggregate(string.Empty, (current, partText) => current + partText);

        account.ExpirationTimes = Math.Max(0, account.ExpirationTimes - 1);
        account.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { expired = false, text = text ?? string.Empty });
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

public class AiHookVideoDescribeRequest
{
    public string Username { get; set; } = string.Empty;
    public string? Prompt { get; set; }
    public IFormFile? Video { get; set; }
}

public record AiHookAccountDto(
    string Username,
    string? ApiKey,
    string PaymentType,
    DateTime ExpirationDate,
    int ExpirationTimes,
    string? BankAccount,
    string? BankName
);
