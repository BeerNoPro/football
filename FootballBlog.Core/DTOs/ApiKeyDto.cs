namespace FootballBlog.Core.DTOs;

public record ApiKeyDto(
    int Id,
    string Provider,
    string KeyMasked,   // "****abcd"
    int Priority,
    bool IsActive,
    int DailyLimit,
    string? Note,
    DateTime CreatedAt
);

public record CreateApiKeyDto(
    string Provider,
    string KeyValue,
    int Priority = 1,
    int DailyLimit = 0,
    string? Note = null
);
