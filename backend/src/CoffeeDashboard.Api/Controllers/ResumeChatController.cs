using System.Net.Http.Json;
using System.Text.Json;
using CoffeeDashboard.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/resume-chat")]
public class ResumeChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ResumeChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required" });
        }

        var apiKey = configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Missing Gemini API key" });
        }

        var prompt = "Bạn là trợ lý của Phạm Thái Nguyên. Trả lời ngắn gọn, lịch sự, và tập trung vào kinh nghiệm, kỹ năng, và dự án trong CV. Nếu câu hỏi ngoài phạm vi, hãy trả lời ngắn gọn và đề nghị liên hệ qua email.";

        var history = request.History?.Select(h => new
        {
            role = h.Role,
            parts = new[] { new { text = h.Text } }
        }).ToList() ?? new List<object>();

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
