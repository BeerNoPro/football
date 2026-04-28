namespace FootballBlog.Core.DTOs;

public record StandingDto(
    int Rank,
    int TeamId,
    string TeamName,
    string? TeamLogo,
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
    string? Status
);
