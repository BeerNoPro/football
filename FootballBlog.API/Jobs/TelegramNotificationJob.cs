using System.Diagnostics;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Logging;

namespace FootballBlog.API.Jobs;

public class TelegramNotificationJob(
    IUnitOfWork uow,
    ITelegramService telegramService,
    ILogger<TelegramNotificationJob> logger)
{
    /// <summary>Gửi prediction mới lên Telegram (gọi sau PublishPredictionJob).</summary>
    public async Task SendPredictionAsync(int predictionId)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("TelegramNotificationJob.SendPrediction started for prediction {PredictionId}", predictionId);

        var prediction = await uow.MatchPredictions.GetByIdAsync(predictionId);
        if (prediction is null)
        {
            logger.LogWarning("Prediction {PredictionId} not found", predictionId);
            return;
        }

        if (prediction.TelegramMessageId.HasValue)
        {
            sw.Stop();
            logger.LogDebug("Prediction {PredictionId} already sent to Telegram, skipping (Duration={DurationMs}ms)",
                predictionId, sw.ElapsedMilliseconds);
            return;
        }

        var match = await uow.Matches.GetWithPredictionAsync(prediction.MatchId);
        if (match is null)
        {
            logger.LogWarning("Match {MatchId} not found for prediction {PredictionId}", prediction.MatchId, predictionId);
            return;
        }

        long? messageId = await telegramService.SendPredictionAsync(prediction, match);
        if (messageId.HasValue)
        {
            prediction.TelegramMessageId = messageId;
            await uow.MatchPredictions.UpdateAsync(prediction);
            await uow.CommitAsync();

            sw.Stop();
            logger.LogInformation(
                "TelegramNotificationJob.SendPrediction finished for prediction {PredictionId}. MessageId={MessageId}, Duration={DurationMs}ms",
                predictionId, messageId, sw.ElapsedMilliseconds);
        }
        else
        {
            sw.Stop();
            logger.LogWarning("TelegramNotificationJob.SendPrediction failed to send prediction {PredictionId}. Duration={DurationMs}ms",
                predictionId, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>Edit message gốc (PreMatch) — thêm phân tích H2 sau khi HT. Idempotent nếu đã edit.</summary>
    public async Task EditHalfTimeAsync(int htPredictionId)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("TelegramNotificationJob.EditHalfTime started for htPrediction {PredictionId}", htPredictionId);

        var htPrediction = await uow.MatchPredictions.GetByIdAsync(htPredictionId);
        if (htPrediction is null)
        {
            logger.LogWarning("HT prediction {PredictionId} not found", htPredictionId);
            return;
        }

        // Lấy PreMatch prediction để có TelegramMessageId
        var preMatchPrediction = await uow.MatchPredictions.GetByMatchAndPhaseAsync(htPrediction.MatchId, PredictionPhase.PreMatch);
        if (preMatchPrediction?.TelegramMessageId is null)
        {
            logger.LogDebug("No Telegram message to edit for match {MatchId} (no PreMatch TelegramMessageId)", htPrediction.MatchId);
            return;
        }

        var match = await uow.Matches.GetWithPredictionAsync(htPrediction.MatchId);
        if (match is null)
        {
            logger.LogWarning("Match {MatchId} not found for HT prediction", htPrediction.MatchId);
            return;
        }

        await telegramService.EditHalfTimeAsync(preMatchPrediction.TelegramMessageId.Value, match, preMatchPrediction, htPrediction);

        sw.Stop();
        logger.LogInformation(
            "TelegramNotificationJob.EditHalfTime finished for match {MatchId}. Duration={DurationMs}ms",
            htPrediction.MatchId, sw.ElapsedMilliseconds);
    }
}
