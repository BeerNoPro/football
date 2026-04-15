namespace FootballBlog.Core.DTOs;

/// <summary>
/// Raw data từ Football API — dùng để upsert Country/League/Team/Match trong FetchUpcomingMatchesJob.
/// Không phải entity, không có DbSet.
/// </summary>
public record FixtureRawDto(
    int ExternalId,
    DateTime KickoffUtc,
    string StatusShort,
    int? HomeScore,
    int? AwayScore,
    string? VenueName,
    string? RefereeName,

    // Team data
    int HomeTeamExternalId,
    string HomeTeamName,
    string? HomeTeamLogo,
    int AwayTeamExternalId,
    string AwayTeamName,
    string? AwayTeamLogo,

    // League data
    int LeagueExternalId,
    string LeagueName,
    string? LeagueLogo,

    // Country data (từ league.country trong API response)
    string CountryName,
    string CountryCode,   // derive từ flag URL hoặc country name
    string? CountryFlag,

    string Season,
    string? Round
);
