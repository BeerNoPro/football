namespace FootballBlog.Core.DTOs;

/// <summary>
/// Raw standing entry từ GET /standings?league=X&amp;season=Y.
/// Dùng để upsert Standing trong SeedLeagueDataJob.
/// </summary>
public record StandingRawDto(
    int LeagueExternalId,
    int Season,
    int TeamExternalId,
    string TeamName,
    int Rank,
    int Points,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalsDiff,
    string? Form,
    string? Description,
    string? Status,
    DateTime UpdatedAt
);
