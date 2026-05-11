namespace FootballBlog.Core.DTOs;

public record MatchStatsDto(
    int Total,
    int Live,
    int WithPrediction,
    int PendingPrediction,
    Dictionary<string, int>? BySeasons = null
);
