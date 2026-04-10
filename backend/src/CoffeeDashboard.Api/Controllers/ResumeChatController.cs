using System.Net.Http.Json;
using System.Text.Json;
using CoffeeDashboard.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/resume-chat")]
public class ResumeChatController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    DashboardDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ResumeChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required" });
        }

        var apiKey = await dbContext.ResumeInfos
            .Where(x => x.ObjectKey == "gemini_api_key")
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        apiKey ??= configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Missing Gemini API key" });
        }

        var prompt =
            "Bạn là trợ lý của anh Nguyên. Không gọi tên đầy đủ, hãy xưng hô tôn trọng và dùng 'anh Nguyên'. Chỉ trả lời dựa trên CV tóm tắt dưới đây. Nếu ngoài phạm vi, hãy nói chưa có thông tin và đề nghị liên hệ email.\n\n" +
            "CV tóm tắt:\n" +
            "- Anh Nguyên · Software Developer · 7+ năm.\n" +
            "- AI: OpenClaw, ChatGPI, Claude.\n" +
            "- Stack: ASP.NET (.NET Framework), C#, C++, AngularJS, JS (ES6), jQuery.\n" +
            "- DB: SQL Server, PostgreSQL.\n" +
            "- Domain: ERP/CRM, kiến trúc, tối ưu hiệu năng DB, mở rộng hệ thống.\n" +
            "- Kinh nghiệm: TILSOFT (ERP nội thất, 2022–2026), RASHINBAN (ERP, 2020–2022), VOIP (Backend .NET Framework + Oracle + Jenkins, 2019–2020), Automation Test Tool (Java+Selenium, 2019), Recochoku (Android/Java, 2018–2019).\n" +
            "- Học vấn: HCMUT · Software Engineering.\n" +
            "- Thành tích: Hutech Olympic 2017 (First Prize); Top 25 Olympic Informatics VN 26th.\n" +
            "- Liên hệ: phamthainguyenit@gmail.com | 0342555919.";

        var history = new List<object>();

        if (request.History != null)
        {
            history.AddRange(request.History.Select(h => new
            {
                role = string.Equals(h.Role, "assistant", StringComparison.OrdinalIgnoreCase) ? "MODEL" : "USER",
                parts = new[] { new { text = h.Text } }
            }));
        }

        history.Insert(0, new { role = "USER", parts = new[] { new { text = prompt } } });
        history.Add(new { role = "USER", parts = new[] { new { text = request.Message.Trim() } } });

        var payload = new { contents = history };
        var httpClient = httpClientFactory.CreateClient("Gemini");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={Uri.EscapeDataString(apiKey)}";

        using var response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return StatusCode((int)response.StatusCode, new { message = "Gemini request failed", details = errorBody });
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

        return Ok(new { text = text ?? string.Empty });
    }
}

public record ResumeChatRequest(string Message, List<ResumeChatHistoryItem>? History);
public record ResumeChatHistoryItem(string Role, string Text);
