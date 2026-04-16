namespace FootballBlog.Core.DTOs;

/// <summary>Dùng cho trang chi tiết bài viết.</summary>
public record PostDetailDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string? Thumbnail,
    string CategoryName,
    string CategorySlug,
    string AuthorName,
    DateTime? PublishedAt,
    IList<string> Tags
);
