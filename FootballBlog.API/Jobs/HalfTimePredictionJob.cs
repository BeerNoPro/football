using System.Diagnostics;
using System.Text.Json;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace FootballBlog.API.Jobs;

public class HalfTimePredictionJob(
    IUnitOfWork uow,
    IFootballApiClient apiClient,
    IEnumerable<IAIPredictionProvider> providers,
    ILogger<HalfTimePredictionJob> logger)
{
    public async Task ExecuteAsync(int matchId)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("HalfTimePredictionJob started for match {MatchId}", matchId);

        // Idempotency: bỏ qua nếu đã có HT prediction
        var existing = await uow.MatchPredictions.GetByMatchAndPhaseAsync(matchId, PredictionPhase.HalfTime);
        if (existing is not null)
        {
            logger.LogDebug("HalfTimePredictionJob: match {MatchId} already has HT prediction, skip", matchId);
            return;
        }

        var match = await uow.Matches.GetWithPredictionAsync(matchId);
        if (match is null)
        {
            logger.LogWarning("HalfTimePredictionJob: match {MatchId} not found", matchId);
            return;
        }

        var contextData = await uow.MatchContexts.GetByMatchIdAsync(matchId);
        if (contextData is null)
        {
            logger.LogWarning("HalfTimePredictionJob: no ContextData for match {MatchId}, skip", matchId);
            return;
        }

        MatchContext? preMatchContext;
        try
        {
            preMatchContext = JsonSerializer.Deserialize<MatchContext>(contextData.ContextJson)
                ?? throw new JsonException("Deserialize returned null");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "HalfTimePredictionJob: cannot deserialize ContextJson for match {MatchId}", matchId);
            return;
        }

        // Lấy tỷ số HT từ LiveMatch
        var liveMatch = await uow.LiveMatches.GetByExternalIdAsync(match.ExternalId);
        int htHome = liveMatch?.HomeScore ?? 0;
        int htAway = liveMatch?.AwayScore ?? 0;

        // Gọi Football API lấy statistics + events H1
        HalfTimeContext? htContext = await apiClient.GetFixtureHalfTimeDataAsync(match.ExternalId, htHome, htAway, match.HomeTeam.ExternalId, match.AwayTeam.ExternalId);
        if (htContext is null)
        {
            logger.LogWarning("HalfTimePredictionJob: failed to fetch HT data for match {MatchId}, skip", matchId);
            return;
        }

        // Gọi AI — Claude primary → Gemini fallback
        IAIPredictionProvider? primary = providers.FirstOrDefault(p => p.ProviderName == "Claude");
        IAIPredictionProvider? fallback = providers.FirstOrDefault(p => p.ProviderName == "Gemini");

        AIPredictionResult? result = null;
        string usedProvider = string.Empty;
        string usedModel = string.Empty;

        if (primary is not null)
        {
            try
            {
                result = await primary.PredictHalfTimeAsync(match, preMatchContext, htContext);
                usedProvider = primary.ProviderName;
                usedModel = primary.ModelName;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Claude HT failed for match {MatchId}, trying Gemini", matchId);
            }
        }

        if (result is null && fallback is not null)
        {
            try
            {
                result = await fallback.PredictHalfTimeAsync(match, preMatchContext, htContext);
                usedProvider = fallback.ProviderName;
                usedModel = fallback.ModelName;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Both providers failed for HT match {MatchId}", matchId);
                return;
            }
        }

        if (result is null)
        {
            return;
        }

        var htPrediction = new MatchPrediction
        {
            MatchId = match.Id,
            Phase = PredictionPhase.HalfTime,
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
            GeneratedAt = DateTime.UtcNow
        };

        await uow.MatchPredictions.AddAsync(htPrediction);
        await uow.CommitAsync();

        BackgroundJob.Enqueue<TelegramNotificationJob>(j => j.EditHalfTimeAsync(htPrediction.Id));

        sw.Stop();
        logger.LogInformation(
            "HalfTimePredictionJob finished for match {MatchId} via {Provider}. HT={HtHome}-{HtAway}, Outcome={Outcome}, Duration={DurationMs}ms",
            matchId, usedProvider, htHome, htAway, result.PredictedOutcome, sw.ElapsedMilliseconds);
    }
}
