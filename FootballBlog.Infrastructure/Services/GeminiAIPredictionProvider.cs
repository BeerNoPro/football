using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Logging;

namespace FootballBlog.Infrastructure.Services;

public class GeminiAIPredictionProvider(
    IHttpClientFactory httpClientFactory,
    IApiKeyRotator keyRotator,
    IApiUsageTracker usageTracker,
    ILogger<GeminiAIPredictionProvider> logger) : IAIPredictionProvider
{
    public string ProviderName => "Gemini";
    public string ModelName => "gemini-2.5-flash";

    public async Task<AIPredictionResult> PredictAsync(Match match, MatchContext context, CancellationToken ct = default)
    {
        var apiKey = await keyRotator.GetAvailableKeyAsync("Gemini")
            ?? throw new InvalidOperationException("Không có Gemini API key khả dụng");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={apiKey}";
        var prompt = BuildPrompt(match, context);

        var requestBody = new
        {
            contents = new[] { new { role = "user", parts = new[] { new { text = prompt } } } }
        };

        if (!await usageTracker.CanCallAsync("Gemini"))
        {
            throw new InvalidOperationException("Gemini daily quota exhausted");
        }

        using var client = httpClientFactory.CreateClient();

        logger.LogDebug("Calling Gemini API for match {MatchId}", match.Id);

        var response = await client.PostAsJsonAsync(url, requestBody, ct);

        if (response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Forbidden)
        {
            await keyRotator.MarkExhaustedAsync("Gemini", apiKey);
        }

        response.EnsureSuccessStatusCode();
        await usageTracker.IncrementAsync("Gemini");

        var raw = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Gemini trả về null response");

        var text = raw.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("Gemini trả về content rỗng");

        var promptTokens = raw.UsageMetadata?.PromptTokenCount ?? 0;
        var completionTokens = raw.UsageMetadata?.CandidatesTokenCount ?? 0;

        return ParseResult(text, promptTokens, completionTokens);
    }

    public async Task<AIPredictionResult> PredictHalfTimeAsync(Match match, MatchContext preMatchContext, HalfTimeContext htContext, CancellationToken ct = default)
    {
        var apiKey = await keyRotator.GetAvailableKeyAsync("Gemini")
            ?? throw new InvalidOperationException("Không có Gemini API key khả dụng");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={apiKey}";
        var prompt = BuildHalfTimePrompt(match, preMatchContext, htContext);
        var requestBody = new
        {
            contents = new[] { new { role = "user", parts = new[] { new { text = prompt } } } }
        };

        if (!await usageTracker.CanCallAsync("Gemini"))
        {
            throw new InvalidOperationException("Gemini daily quota exhausted");
        }

        using var client = httpClientFactory.CreateClient();
        logger.LogDebug("Calling Gemini API for HT prediction match {MatchId}", match.Id);

        var response = await client.PostAsJsonAsync(url, requestBody, ct);

        if (response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Forbidden)
        {
            await keyRotator.MarkExhaustedAsync("Gemini", apiKey);
        }

        response.EnsureSuccessStatusCode();
        await usageTracker.IncrementAsync("Gemini");

        var raw = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Gemini trả về null response");

        var text = raw.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("Gemini trả về content rỗng");

        return ParseResult(text, raw.UsageMetadata?.PromptTokenCount ?? 0, raw.UsageMetadata?.CandidatesTokenCount ?? 0);
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

    private static string BuildHalfTimePrompt(Match match, MatchContext preMatch, HalfTimeContext ht)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Phân tích hiệp 2: {match.HomeTeam.Name} vs {match.AwayTeam.Name} | {match.League.Name}");
        sb.AppendLine($"Tỷ số HT: {ht.HtHomeScore}-{ht.HtAwayScore}");
        sb.AppendLine($"Kiểm soát bóng: Nhà {ht.HomePossession ?? "?"} | Khách {ht.AwayPossession ?? "?"}");
        sb.AppendLine($"Sút trúng đích: Nhà {ht.HomeShotsOnTarget ?? 0} | Khách {ht.AwayShotsOnTarget ?? 0}");
        sb.AppendLine($"Phạt góc: Nhà {ht.HomeCorners ?? 0} | Khách {ht.AwayCorners ?? 0}");
        sb.AppendLine($"Thẻ vàng: Nhà {ht.HomeYellowCards ?? 0} | Khách {ht.AwayYellowCards ?? 0}");

        if (ht.H1Events.Count > 0)
        {
            sb.AppendLine("Sự kiện H1: " + string.Join(", ", ht.H1Events.Select(e => $"{e.Minute}'{e.Type}({e.Team})")));
        }

        sb.AppendLine($"H2H: Nhà {preMatch.H2H.HomeWins}W-{preMatch.H2H.Draws}D-{preMatch.H2H.AwayWins}W Khách");
        sb.AppendLine($"Phong độ nhà: {preMatch.HomeForm.FormString} | Khách: {preMatch.AwayForm.FormString}");
        sb.AppendLine();
        sb.AppendLine("Phân tích và dự đoán hiệp 2. Trả về JSON hợp lệ (không markdown fence):");
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
            CompletionTokens: completionTokens,
            RawResponse: text
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
