namespace FootballBlog.Core.DTOs;

public record PromptTemplateDto(
    int Id,
    string Name,
    string Provider,
    string Content,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreatePromptTemplateDto(
    string Name,
    string Provider,
    string Content,
    bool IsActive
);
