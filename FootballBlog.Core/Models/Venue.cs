namespace FootballBlog.Core.Models;

public class Venue
{
    public int Id { get; set; }

    /// <summary>Venue ID từ api-football.</summary>
    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public int? Capacity { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<Team> Teams { get; set; } = [];
}
