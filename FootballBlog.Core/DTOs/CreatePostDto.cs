namespace FootballBlog.Core.DTOs;

/// <summary>Dùng cho tạo mới và cập nhật bài viết (Admin).</summary>
public record CreatePostDto(
    string Title,
    string Slug,
    string Content,
    string? Thumbnail,
    int CategoryId,
    int AuthorId,
    bool PublishNow = false
);
