using FootballBlog.Core.Models;

namespace FootballBlog.Core.DTOs;

public record FixtureDto(
    int Id,
    int ExternalId,
    int LeagueId,
    string LeagueName,
    string? LeagueLogo,
    string CountryName,
    string? CountryFlag,
    string Season,
    string? Round,
    DateTime KickoffUtc,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore,
    int HomeTeamId,
    string HomeTeamName,
    string? HomeTeamLogo,
    int AwayTeamId,
    string AwayTeamName,
    string? AwayTeamLogo,
    string? VenueName,
    bool HasPrediction
);

public record FixtureSuggestDto(int Id, string HomeTeam, string AwayTeam);
