namespace FootballBlog.Core.DTOs;

public record LiveMatchDto(
    int Id,
    int ExternalId,
    string HomeTeam,
    string AwayTeam,
    int HomeScore,
    int AwayScore,
    string Status,
    int? Minute,
    DateTime StartedAt,
    IList<MatchEventDto> Events
);
