namespace FootballBlog.Core.DTOs;

public record MatchEventDto(
    int Id,
    int Minute,
    string Type,
    string Description
);
