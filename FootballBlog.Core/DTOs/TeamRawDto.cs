namespace FootballBlog.Core.DTOs;

/// <summary>
/// Raw team + venue data từ GET /teams?league=X&amp;season=Y.
/// Dùng để upsert Team + Venue trong SeedLeagueDataJob.
/// </summary>
public record TeamRawDto(
    int TeamExternalId,
    string TeamName,
    string? TeamCode,
    string? TeamLogo,
    string? CountryName,

    int? VenueExternalId,
    string? VenueName,
    string? VenueCity,
    int? VenueCapacity,
    string? VenueImageUrl
);
