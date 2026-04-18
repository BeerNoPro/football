namespace FootballBlog.Core.DTOs;

public record PredictionStatsDto(
    int Total,
    int Published,
    int Pending,
    int TodayCount,
    decimal AccuracyPercent
);
