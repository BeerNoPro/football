namespace FootballBlog.Core.DTOs;

/// <summary>Dùng cho trang danh sách — không load Content để tránh over-fetching.</summary>
public record PostSummaryDto(
    int Id,
    string Title,
    string Slug,
    string? Thumbnail,
    string CategoryName,
    string CategorySlug,
    string AuthorName,
    DateTime PublishedAt
);
