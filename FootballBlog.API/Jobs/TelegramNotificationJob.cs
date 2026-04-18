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

    /// <summary>Edit Telegram message sau khi trận kết thúc với kết quả thực tế.</summary>
    public async Task SendResultAsync(int matchId)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("TelegramNotificationJob.SendResult started for match {MatchId}", matchId);

        var match = await uow.Matches.GetWithPredictionAsync(matchId);
        if (match?.Prediction is null)
        {
            logger.LogWarning("Match {MatchId} or prediction not found", matchId);
            return;
        }

        if (!match.Prediction.TelegramMessageId.HasValue)
        {
            sw.Stop();
            logger.LogDebug("No Telegram message to edit for match {MatchId}. Duration={DurationMs}ms",
                matchId, sw.ElapsedMilliseconds);
            return;
        }

        if (match.HomeScore is null || match.AwayScore is null)
        {
            logger.LogWarning("Match {MatchId} result not available yet", matchId);
            return;
        }

        await telegramService.EditResultAsync(match.Prediction.TelegramMessageId.Value, match, match.Prediction);

        sw.Stop();
        logger.LogInformation(
            "TelegramNotificationJob.SendResult finished for match {MatchId}. Result={Result}, Duration={DurationMs}ms",
            matchId, $"{match.HomeScore}-{match.AwayScore}", sw.ElapsedMilliseconds);
    }
}
