using System.Diagnostics;
using System.Text.Json;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace FootballBlog.API.Jobs;

public class GeneratePredictionJob(
    IUnitOfWork uow,
    IEnumerable<IAIPredictionProvider> providers,
    ILogger<GeneratePredictionJob> logger)
{
    private static readonly TimeZoneInfo VnZone = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");

    /// <summary>
    /// Trigger per-match từ PreMatchDataJob.FetchH2HAsync — đây là path chính.
    /// Load match + context riêng để tránh bug ContextData = null khi dùng batch query.
    /// </summary>
    public async Task ExecuteForMatchAsync(int matchId)
    {
        var sw = Stopwatch.StartNew();

        Match? match = await uow.Matches.GetWithPredictionAsync(matchId);
        if (match is null)
        {
            logger.LogWarning("GeneratePredictionJob: match {MatchId} not found", matchId);
            return;
        }

        if (match.Predictions.Any(p => p.Phase == PredictionPhase.PreMatch))
        {
            logger.LogDebug("GeneratePredictionJob: match {MatchId} already has PreMatch prediction, skip", matchId);
            return;
        }

        if (match.KickoffUtc <= DateTime.UtcNow)
        {
            logger.LogWarning("GeneratePredictionJob: match {MatchId} already started or finished, skip", matchId);
            return;
        }

        MatchContextData? contextData = await uow.MatchContexts.GetByMatchIdAsync(matchId);
        if (contextData is null)
        {
            logger.LogWarning("GeneratePredictionJob: no ContextData for match {MatchId}, skip", matchId);
            return;
        }

        MatchContext? context = DeserializeContext(contextData.ContextJson, matchId);
        if (context is null)
        {
            return;
        }

        await RunAndScheduleAsync(match, context);

        sw.Stop();
        logger.LogInformation(
            "GeneratePredictionJob.ExecuteForMatch finished for match {MatchId}. Duration={DurationMs}ms",
            matchId, sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Batch job — trigger thủ công qua /hangfire hoặc RecurringJob hourly.
    /// Xử lý tất cả trận Scheduled chưa có prediction và đã có ContextData.
    /// </summary>
    public async Task ExecuteAsync()
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("GeneratePredictionJob batch started at {StartTime}", DateTime.UtcNow);

        IEnumerable<Match> matches = await uow.Matches.GetWithoutPredictionAsync();
        List<Match> candidates = matches
            .Where(m => m.KickoffUtc > DateTime.UtcNow)
            .ToList();

        if (candidates.Count == 0)
        {
            logger.LogInformation("GeneratePredictionJob batch finished — no candidates. Duration={DurationMs}ms", sw.ElapsedMilliseconds);
            return;
        }

        int generated = 0;
        int skipped = 0;

        foreach (Match match in candidates)
        {
            // GetWithoutPredictionAsync không include ContextData — load riêng
            MatchContextData? contextData = await uow.MatchContexts.GetByMatchIdAsync(match.Id);
            if (contextData is null)
            {
                skipped++;
                continue;
            }

            MatchContext? context = DeserializeContext(contextData.ContextJson, match.Id);
            if (context is null) { skipped++; continue; }

            bool ok = await RunAndScheduleAsync(match, context);
            if (ok)
            {
                generated++;
            }
            else
            {
                skipped++;
            }
        }

        sw.Stop();
        logger.LogInformation(
            "GeneratePredictionJob batch finished. Generated={Generated}, Skipped={Skipped}, Duration={DurationMs}ms",
            generated, skipped, sw.ElapsedMilliseconds);
    }

    // ── Shared logic ──────────────────────────────────────────────────────────

    private async Task<bool> RunAndScheduleAsync(Match match, MatchContext context)
    {
        IAIPredictionProvider? primary = providers.FirstOrDefault(p => p.ProviderName == "Claude");
        IAIPredictionProvider? fallback = providers.FirstOrDefault(p => p.ProviderName == "Gemini");

        AIPredictionResult? result = null;
        string usedProvider = string.Empty;
        string usedModel = string.Empty;

        if (primary is not null)
        {
            try
            {
                result = await primary.PredictAsync(match, context);
                usedProvider = primary.ProviderName;
                usedModel = primary.ModelName;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Claude failed for match {MatchId}, trying Gemini", match.Id);
            }
        }

        if (result is null && fallback is not null)
        {
            try
            {
                result = await fallback.PredictAsync(match, context);
                usedProvider = fallback.ProviderName;
                usedModel = fallback.ModelName;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Both providers failed for match {MatchId}", match.Id);
                return false;
            }
        }

        if (result is null)
        {
            return false;
        }

        MatchPrediction prediction = new()
        {
            MatchId = match.Id,
            Phase = PredictionPhase.PreMatch,
            AIProvider = usedProvider,
            AIModel = usedModel,
            PredictedOutcome = result.PredictedOutcome,
            PredictedHomeScore = result.PredictedHomeScore,
            PredictedAwayScore = result.PredictedAwayScore,
            ConfidenceScore = result.ConfidenceScore,
            AnalysisSummary = result.AnalysisSummary,
            RawResponse = result.RawResponse,
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
            GeneratedAt = DateTime.UtcNow,
            IsPublished = false
        };

        await uow.MatchPredictions.AddAsync(prediction);
        await uow.CommitAsync();

        // Gửi Telegram lúc 06:00 VN hôm nay, hoặc ngày mai nếu đã qua
        DateTime nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VnZone);
        DateTime sendAtVn = nowVn.Date.AddHours(6);
        if (sendAtVn <= nowVn)
        {
            sendAtVn = sendAtVn.AddDays(1);
        }

        DateTime sendAtUtc = TimeZoneInfo.ConvertTimeToUtc(sendAtVn, VnZone);

        BackgroundJob.Schedule<TelegramNotificationJob>(j => j.SendPredictionAsync(prediction.Id), sendAtUtc);

        logger.LogInformation(
            "Prediction generated for match {MatchId} via {Provider}: {Outcome} ({Confidence}%), TelegramAt={SendAt} VN",
            match.Id, usedProvider, result.PredictedOutcome, result.ConfidenceScore, sendAtVn);

        return true;
    }

    private MatchContext? DeserializeContext(string json, int matchId)
    {
        try
        {
            return JsonSerializer.Deserialize<MatchContext>(json)
                ?? throw new JsonException("Deserialize returned null");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cannot deserialize ContextJson for match {MatchId}", matchId);
            return null;
        }
    }
}
