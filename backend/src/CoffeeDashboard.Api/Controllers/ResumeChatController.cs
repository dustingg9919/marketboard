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
            "Bạn là trợ lý của anh Nguyên. Không gọi tên đầy đủ, hãy xưng hô tôn trọng và dùng 'anh Nguyên'. Chỉ trả lời dựa trên CV đầy đủ dưới đây. Tuyệt đối không tự suy diễn. Nếu ngoài phạm vi, hãy nói chưa có thông tin và đề nghị liên hệ email.\n\n" +
            "CV đầy đủ (theo trang resume):\n" +
            "- Họ tên: Phạm Thái Nguyên. Vị trí: Software Developer. Kinh nghiệm: 7+ years.\n" +
            "- Contact: 0342555919 | phamthainguyenit@gmail.com.\n" +
            "- Skills: AI (Copilot, OpenClaw, ChatGPI, Claude); Backend (ASP.NET, .NET Core, C#, C++); Frontend (AngularJS, JavaScript ES6, jQuery); Databases (SQL Server, PostgreSQL); English (Read and write technical documents and requirements).\n" +
            "- Education: Ho Chi Minh City University of Technology (HUTECH) — Major: Software Engineering. Chỉ dùng đúng tên này, không thay bằng trường khác.\n" +
            "- Achievement: First Prize of Hutech Olympic Programming Contest 2017; Top 25 Olympic Informatics students Vietnam 26th.\n" +
            "- Personal Interests: Game, Badminton, Hiking.\n" +
            "- Highlights: Full-stack developer with 7+ years building enterprise web; specialized in ASP.NET, .NET Core and AngularJS; strong ERP/CRM experience; 2+ years experience with AI tools; scalable architecture, DB performance optimization; system scalability & clean architecture.\n" +
            "- Experience:\n" +
            "  * TILSOFT (AUG 2022 – JAN 2026) — FULL STACK DEVELOPER. Furniture Industry ERP Platform.\n" +
            "    Responsibilities: Designed and implemented ERP module architecture; developed backend services using .NET Core with modular monolith architecture; auth/authorization; API performance; AngularJS + Bootstrap SPA; managed & optimized SQL Server; improved query performance; collaborated cross-functional.\n" +
            "    Technologies: Backend .NET Core; Frontend AngularJS, Bootstrap; Database SQL Server; Devops CI/CD pipeline.\n" +
            "  * RASHINBAN (OCT 2020 – JUL 2022) — FULL STACK DEVELOPER. Enterprise resource management system.\n" +
            "    Responsibilities: Backend APIs + frontend modules (.NET + AngularJS); optimized SQL; resolved production issues; customer support; testing/debugging; mentored new members; training docs.\n" +
            "    Technologies: Backend ASP.NET; Frontend AngularJS, Bootstrap; Databases SQL Server/PostgreSQL; Tools Git, Redmine, Visual Studio.\n" +
            "  * VOIP PROJECT (APR 2019 – SEP 2020) — BACKEND DEVELOPER.\n" +
            "    Responsibilities: Developed backend services using .NET Framework; managed Oracle DB; production support; implemented CI/CD with Jenkins.\n" +
            "    Technologies: .NET Framework; Oracle Database; Jenkins, Linux, Shell Script; Gradle.\n" +
            "  * AUTOMATION TEST TOOL (MAR 2019 – APR 2019) — FULL STACK DEVELOPER.\n" +
            "    Responsibilities: Designed automated testing framework; generated test cases from Excel; implemented Selenium scripts.\n" +
            "    Technologies: Java, Selenium, PostgreSQL.\n" +
            "  * RECOCHOKU PROJECT (MAY 2018 – MAR 2019) — ANDROID DEVELOPER.\n" +
            "    Responsibilities: Developed Android features for music/video; search & playlist; bug fixing.\n" +
            "    Technologies: Java (Android), MediaPlayer/VideoView, PostgreSQL.";

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
