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
    public async Task ExecuteAsync()
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("GeneratePredictionJob started at {StartTime}", DateTime.UtcNow);

        var matches = await uow.Matches.GetWithoutPredictionAsync();
        var candidates = matches
            .Where(m => m.ContextData is not null && m.KickoffUtc > DateTime.UtcNow)
            .ToList();

        logger.LogDebug("Found {CandidateCount} matches ready for prediction", candidates.Count);

        if (candidates.Count == 0)
        {
            logger.LogInformation("GeneratePredictionJob finished. No candidates. Duration={DurationMs}ms", sw.ElapsedMilliseconds);
            return;
        }

        var primary = providers.FirstOrDefault(p => p.ProviderName == "Claude");
        var fallback = providers.FirstOrDefault(p => p.ProviderName == "Gemini");

        int generated = 0;
        int failed = 0;

        foreach (Match match in candidates)
        {
            MatchContext context;
            try
            {
                context = JsonSerializer.Deserialize<MatchContext>(match.ContextData!.ContextJson)
                    ?? throw new JsonException("Deserialize trả về null");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Không thể deserialize ContextJson cho match {MatchId}", match.Id);
                failed++;
                continue;
            }

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
                    logger.LogWarning(ex, "Claude thất bại cho match {MatchId}, thử Gemini", match.Id);
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
                    logger.LogError(ex, "Cả 2 providers đều thất bại cho match {MatchId}", match.Id);
                    continue;
                }
            }

            if (result is null)
            {
                continue;
            }

            var prediction = new MatchPrediction
            {
                MatchId = match.Id,
                AIProvider = usedProvider,
                AIModel = usedModel,
                PredictedOutcome = result.PredictedOutcome,
                PredictedHomeScore = result.PredictedHomeScore,
                PredictedAwayScore = result.PredictedAwayScore,
                ConfidenceScore = result.ConfidenceScore,
                AnalysisSummary = result.AnalysisSummary,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
                GeneratedAt = DateTime.UtcNow,
                IsPublished = false
            };

            await uow.MatchPredictions.AddAsync(prediction);
            await uow.CommitAsync();

            BackgroundJob.Enqueue<PublishPredictionJob>(j => j.ExecuteAsync(prediction.Id));

            generated++;
            logger.LogInformation(
                "Prediction generated for match {MatchId} via {Provider}: {Outcome} ({Confidence}%)",
                match.Id, usedProvider, result.PredictedOutcome, result.ConfidenceScore);
        }

        sw.Stop();
        logger.LogInformation(
            "GeneratePredictionJob finished. Generated={Generated}, Failed={Failed}, Duration={DurationMs}ms",
            generated, failed, sw.ElapsedMilliseconds);
    }
}
