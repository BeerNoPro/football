namespace FootballBlog.Core.DTOs;

public record SquadPlayerDto(
    int ExternalId,
    string Name,
    int? Age,
    int? Number,
    string? Position,
    string? Photo
);
