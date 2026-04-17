using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FootballBlog.Infrastructure.Services;

public class ClaudeAIPredictionProvider(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<ClaudeAIPredictionProvider> logger) : IAIPredictionProvider
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    public string ProviderName => "Claude";
    public string ModelName => "claude-opus-4-6";

    public async Task<AIPredictionResult> PredictAsync(Match match, MatchContext context, CancellationToken ct = default)
    {
        var apiKey = configuration["Claude:ApiKey"]
            ?? throw new InvalidOperationException("Claude:ApiKey chưa được cấu hình");

        var prompt = BuildPrompt(match, context);

        var requestBody = new
        {
            model = ModelName,
            max_tokens = 1024,
            messages = new[] { new { role = "user", content = prompt } }
        };

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        logger.LogDebug("Calling Claude API for match {MatchId}", match.Id);

        var response = await client.PostAsJsonAsync(ApiUrl, requestBody, ct);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<ClaudeResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Claude trả về null response");

        var text = raw.Content.FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("Claude trả về content rỗng");

        return ParseResult(text, raw.Usage.InputTokens, raw.Usage.OutputTokens);
    }

    private static string BuildPrompt(Match match, MatchContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Phân tích và dự đoán trận đấu bóng đá sau đây. Trả lời bằng JSON hợp lệ.");
        sb.AppendLine();
        sb.AppendLine($"## Thông tin trận đấu");
        sb.AppendLine($"- Đội nhà: {match.HomeTeam.Name}");
        sb.AppendLine($"- Đội khách: {match.AwayTeam.Name}");
        sb.AppendLine($"- Giải đấu: {match.League.Name}");
        sb.AppendLine($"- Thời gian: {match.KickoffUtc:yyyy-MM-dd HH:mm} UTC");
        if (!string.IsNullOrEmpty(match.RefereeName))
        {
            sb.AppendLine($"- Trọng tài: {match.RefereeName}");
        }

        sb.AppendLine();

        sb.AppendLine($"## Đối đầu trực tiếp (H2H) — {context.H2H.RecentMatches.Count} trận gần nhất");
        sb.AppendLine($"Thắng đội nhà: {context.H2H.HomeWins} | Hòa: {context.H2H.Draws} | Thắng đội khách: {context.H2H.AwayWins}");
        foreach (var m in context.H2H.RecentMatches)
        {
            sb.AppendLine($"  {m.Date:yyyy-MM-dd} {m.HomeTeam} {m.HomeScore}-{m.AwayScore} {m.AwayTeam} ({m.Competition})");
        }

        sb.AppendLine();

        sb.AppendLine($"## Phong độ đội nhà ({context.HomeForm.TeamName}) — {context.HomeForm.FormString}");
        foreach (var m in context.HomeForm.RecentMatches)
        {
            sb.AppendLine($"  {m.Date:yyyy-MM-dd} vs {m.Opponent} ({(m.IsHome ? "Nhà" : "Khách")}) {m.GoalsFor}-{m.GoalsAgainst} [{m.Result}]");
        }

        sb.AppendLine();

        sb.AppendLine($"## Phong độ đội khách ({context.AwayForm.TeamName}) — {context.AwayForm.FormString}");
        foreach (var m in context.AwayForm.RecentMatches)
        {
            sb.AppendLine($"  {m.Date:yyyy-MM-dd} vs {m.Opponent} ({(m.IsHome ? "Nhà" : "Khách")}) {m.GoalsFor}-{m.GoalsAgainst} [{m.Result}]");
        }

        sb.AppendLine();

        if (context.Lineup is not null)
        {
            sb.AppendLine("## Đội hình dự kiến");
            if (context.Lineup.HomeProbableXI.Count > 0)
            {
                sb.AppendLine($"  Đội nhà: {string.Join(", ", context.Lineup.HomeProbableXI)}");
            }

            if (context.Lineup.AwayProbableXI.Count > 0)
            {
                sb.AppendLine($"  Đội khách: {string.Join(", ", context.Lineup.AwayProbableXI)}");
            }

            if (context.Lineup.HomeInjuries.Count > 0)
            {
                sb.AppendLine($"  Vắng mặt đội nhà: {string.Join(", ", context.Lineup.HomeInjuries)}");
            }

            if (context.Lineup.AwayInjuries.Count > 0)
            {
                sb.AppendLine($"  Vắng mặt đội khách: {string.Join(", ", context.Lineup.AwayInjuries)}");
            }

            sb.AppendLine();
        }

        if (context.Fatigue is not null)
        {
            sb.AppendLine("## Thể lực");
            if (context.Fatigue.HomeDaysSinceLastMatch.HasValue)
            {
                sb.AppendLine($"  Đội nhà: {context.Fatigue.HomeDaysSinceLastMatch} ngày nghỉ");
            }

            if (context.Fatigue.AwayDaysSinceLastMatch.HasValue)
            {
                sb.AppendLine($"  Đội khách: {context.Fatigue.AwayDaysSinceLastMatch} ngày nghỉ");
            }

            if (!string.IsNullOrEmpty(context.Fatigue.Notes))
            {
                sb.AppendLine($"  Ghi chú: {context.Fatigue.Notes}");
            }

            sb.AppendLine();
        }

        sb.AppendLine("## Yêu cầu đầu ra");
        sb.AppendLine("Trả về JSON hợp lệ (không có markdown fence) theo đúng cấu trúc sau:");
        sb.AppendLine(@"{
  ""predictedOutcome"": ""HomeWin"" | ""Draw"" | ""AwayWin"",
  ""predictedHomeScore"": <số nguyên>,
  ""predictedAwayScore"": <số nguyên>,
  ""confidenceScore"": <0-100>,
  ""analysisSummary"": ""<phân tích đầy đủ bằng tiếng Việt, định dạng markdown>""
}");

        return sb.ToString();
    }

    private static AIPredictionResult ParseResult(string text, int promptTokens, int completionTokens)
    {
        // Xóa markdown fence nếu có
        var json = text.Trim();
        if (json.StartsWith("```"))
        {
            json = json[json.IndexOf('\n')..].TrimStart();
        }

        if (json.EndsWith("```"))
        {
            json = json[..json.LastIndexOf("```")].TrimEnd();
        }

        var doc = JsonDocument.Parse(json.Trim());
        var root = doc.RootElement;

        return new AIPredictionResult(
            PredictedOutcome: root.GetProperty("predictedOutcome").GetString() ?? "Draw",
            PredictedHomeScore: root.TryGetProperty("predictedHomeScore", out var h) ? h.GetInt32() : null,
            PredictedAwayScore: root.TryGetProperty("predictedAwayScore", out var a) ? a.GetInt32() : null,
            ConfidenceScore: root.TryGetProperty("confidenceScore", out var c) ? c.GetDecimal() : 50m,
            AnalysisSummary: root.GetProperty("analysisSummary").GetString() ?? string.Empty,
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens
        );
    }

    private record ClaudeResponse(
        [property: JsonPropertyName("content")] List<ContentBlock> Content,
        [property: JsonPropertyName("usage")] Usage Usage);

    private record ContentBlock(
        [property: JsonPropertyName("text")] string Text);

    private record Usage(
        [property: JsonPropertyName("input_tokens")] int InputTokens,
        [property: JsonPropertyName("output_tokens")] int OutputTokens);
}
