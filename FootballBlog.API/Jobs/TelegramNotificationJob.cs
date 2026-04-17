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
        logger.LogInformation("TelegramNotificationJob.SendPrediction started for {PredictionId}", predictionId);

        var prediction = await uow.MatchPredictions.GetByIdAsync(predictionId);
        if (prediction is null)
        {
            logger.LogWarning("Prediction {PredictionId} not found", predictionId);
            return;
        }

        if (prediction.TelegramMessageId.HasValue)
        {
            logger.LogDebug("Prediction {PredictionId} already sent to Telegram", predictionId);
            return;
        }

        var match = await uow.Matches.GetWithPredictionAsync(prediction.MatchId);
        if (match is null)
        {
            logger.LogWarning("Match {MatchId} not found", prediction.MatchId);
            return;
        }

        long? messageId = await telegramService.SendPredictionAsync(prediction, match);
        if (messageId.HasValue)
        {
            prediction.TelegramMessageId = messageId;
            await uow.MatchPredictions.UpdateAsync(prediction);
            await uow.CommitAsync();
        }

        logger.LogInformation("TelegramNotificationJob.SendPrediction finished for {PredictionId}", predictionId);
    }

    /// <summary>Edit Telegram message sau khi trận kết thúc với kết quả thực tế.</summary>
    public async Task SendResultAsync(int matchId)
    {
        logger.LogInformation("TelegramNotificationJob.SendResult started for match {MatchId}", matchId);

        var match = await uow.Matches.GetWithPredictionAsync(matchId);
        if (match?.Prediction is null)
        {
            logger.LogWarning("Match {MatchId} or prediction not found", matchId);
            return;
        }

        if (!match.Prediction.TelegramMessageId.HasValue)
        {
            logger.LogDebug("No Telegram message to edit for match {MatchId}", matchId);
            return;
        }

        if (match.HomeScore is null || match.AwayScore is null)
        {
            logger.LogWarning("Match {MatchId} result not available yet", matchId);
            return;
        }

        await telegramService.EditResultAsync(match.Prediction.TelegramMessageId.Value, match, match.Prediction);

        logger.LogInformation("TelegramNotificationJob.SendResult finished for match {MatchId}", matchId);
    }
}
