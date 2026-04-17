using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FootballBlog.Infrastructure.Services;

public class GeminiAIPredictionProvider(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<GeminiAIPredictionProvider> logger) : IAIPredictionProvider
{
    public string ProviderName => "Gemini";
    public string ModelName => "gemini-2.0-flash";

    public async Task<AIPredictionResult> PredictAsync(Match match, MatchContext context, CancellationToken ct = default)
    {
        var apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini:ApiKey chưa được cấu hình");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={apiKey}";
        var prompt = BuildPrompt(match, context);

        var requestBody = new
        {
            contents = new[] { new { role = "user", parts = new[] { new { text = prompt } } } }
        };

        using var client = httpClientFactory.CreateClient();

        logger.LogDebug("Calling Gemini API for match {MatchId}", match.Id);

        var response = await client.PostAsJsonAsync(url, requestBody, ct);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Gemini trả về null response");

        var text = raw.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("Gemini trả về content rỗng");

        var promptTokens = raw.UsageMetadata?.PromptTokenCount ?? 0;
        var completionTokens = raw.UsageMetadata?.CandidatesTokenCount ?? 0;

        return ParseResult(text, promptTokens, completionTokens);
    }

    private static string BuildPrompt(Match match, MatchContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Phân tích và dự đoán trận đấu bóng đá: {match.HomeTeam.Name} vs {match.AwayTeam.Name}");
        sb.AppendLine($"Giải: {match.League.Name} | Thời gian: {match.KickoffUtc:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine();
        sb.AppendLine($"H2H: Nhà {context.H2H.HomeWins}W - {context.H2H.Draws}D - {context.H2H.AwayWins}W Khách");
        sb.AppendLine($"Phong độ nhà ({context.HomeForm.TeamName}): {context.HomeForm.FormString}");
        sb.AppendLine($"Phong độ khách ({context.AwayForm.TeamName}): {context.AwayForm.FormString}");

        if (context.Lineup?.HomeInjuries.Count > 0)
        {
            sb.AppendLine($"Vắng nhà: {string.Join(", ", context.Lineup.HomeInjuries)}");
        }

        if (context.Lineup?.AwayInjuries.Count > 0)
        {
            sb.AppendLine($"Vắng khách: {string.Join(", ", context.Lineup.AwayInjuries)}");
        }

        sb.AppendLine();
        sb.AppendLine("Trả về JSON hợp lệ (không markdown fence):");
        sb.AppendLine(@"{""predictedOutcome"":""HomeWin|Draw|AwayWin"",""predictedHomeScore"":0,""predictedAwayScore"":0,""confidenceScore"":0,""analysisSummary"":""...""}");

        return sb.ToString();
    }

    private static AIPredictionResult ParseResult(string text, int promptTokens, int completionTokens)
    {
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

    private record GeminiResponse(
        [property: JsonPropertyName("candidates")] List<Candidate> Candidates,
        [property: JsonPropertyName("usageMetadata")] UsageMetadata? UsageMetadata);

    private record Candidate(
        [property: JsonPropertyName("content")] CandidateContent Content);

    private record CandidateContent(
        [property: JsonPropertyName("parts")] List<Part> Parts);

    private record Part(
        [property: JsonPropertyName("text")] string Text);

    private record UsageMetadata(
        [property: JsonPropertyName("promptTokenCount")] int PromptTokenCount,
        [property: JsonPropertyName("candidatesTokenCount")] int CandidatesTokenCount);
}
