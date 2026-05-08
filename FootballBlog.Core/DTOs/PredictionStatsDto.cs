namespace FootballBlog.Core.DTOs;

public record PredictionStatsDto(
    int Total,
    int TelegramSent,
    int TodayCount,
    decimal AccuracyPercent
);
