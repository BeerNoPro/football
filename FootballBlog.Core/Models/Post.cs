namespace FootballBlog.Core.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int AuthorId { get; set; }
    public ApplicationUser Author { get; set; } = null!;

    // Null = draft, có giá trị = đã published
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPublished => PublishedAt.HasValue;

    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
