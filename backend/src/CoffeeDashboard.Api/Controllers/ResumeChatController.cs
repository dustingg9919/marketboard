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
            "Bạn là trợ lý của Phạm Thái Nguyên. Chỉ trả lời dựa trên CV dưới đây. Nếu câu hỏi ngoài phạm vi CV, hãy nói bạn chưa có thông tin và đề nghị liên hệ qua email.\n\n" +
            "CV (tóm tắt):\n" +
            "- Họ tên: Phạm Thái Nguyên. Vị trí: Software Developer. Kinh nghiệm: 7+ năm.\n" +
            "- Chuyên môn: ASP.NET (.NET Framework 4.5), C#, C++, AngularJS, JavaScript (ES6), jQuery.\n" +
            "- Cơ sở dữ liệu: SQL Server, PostgreSQL.\n" +
            "- Summary: Full-stack developer, chuyên ERP/CRM, thiết kế kiến trúc, tối ưu hiệu năng DB, hệ thống mở rộng.\n" +
            "- TILSOFT (Aug 2022 – Jan 2026): Full Stack Developer, ERP ngành nội thất. Thiết kế kiến trúc, backend ASP.NET, auth, tối ưu API, AngularJS/Bootstrap, tối ưu SQL.\n" +
            "- RASHINBAN (Oct 2020 – Jul 2022): Full Stack Developer, ERP business. Backend/Frontend .NET + AngularJS, tối ưu SQL, support khách hàng, mentor.\n" +
            "- VOIP Project (Apr 2019 – Sep 2020): Backend Developer (Java J2EE/REST), Oracle, Jenkins CI/CD.\n" +
            "- Automation Test Tool (Mar 2019 – Apr 2019): Java + Selenium, sinh test từ Excel.\n" +
            "- Recochoku (May 2018 – Mar 2019): Android/Java, MediaPlayer/VideoView, PostgreSQL.\n" +
            "- Học vấn: HCM City University of Technology, Software Engineering.\n" +
            "- Thành tích: Hutech Olympic Programming Contest 2017 (First Prize); Top 25 Olympic Informatics Vietnam 26th.\n" +
            "- Liên hệ: phamthainguyenit@gmail.com, 0342555919.";

        var history = new List<object>();

        if (request.History != null)
        {
            history.AddRange(request.History.Select(h => new
            {
                role = h.Role,
                parts = new[] { new { text = h.Text } }
            }));
        }

        history.Insert(0, new { role = "user", parts = new[] { new { text = prompt } } });
        history.Add(new { role = "user", parts = new[] { new { text = request.Message.Trim() } } });

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
