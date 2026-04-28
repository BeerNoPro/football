namespace FootballBlog.Core.DTOs;

public record LeagueDto(
    int Id,
    int ExternalId,
    string Name,
    string? LogoUrl,
    string CountryName,
    string? CountryCode,
    string? CountryFlag
);
