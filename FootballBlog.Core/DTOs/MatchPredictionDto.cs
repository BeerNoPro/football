namespace FootballBlog.Core.DTOs;

public record MatchPredictionDto(
    int Id,
    int MatchId,
    string HomeTeam,
    string AwayTeam,
    string LeagueName,
    DateTime KickoffUtc,
    string AIProvider,
    string AIModel,
    int? PredictedHomeScore,
    int? PredictedAwayScore,
    string PredictedOutcome,
    decimal ConfidenceScore,
    string AnalysisSummary,
    DateTime GeneratedAt,
    bool IsPublished,
    int? BlogPostId
);
