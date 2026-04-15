namespace FootballBlog.Core.Models;

public class League
{
    public int Id { get; set; }

    /// <summary>League ID từ api-football.</summary>
    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;   // "Premier League"
    public string? LogoUrl { get; set; }
    public int CountryId { get; set; }

    /// <summary>false = archive giải đấu không theo dõi nữa (soft-disable, giữ FK cho Matches cũ).</summary>
    public bool IsActive { get; set; } = true;

    public Country Country { get; set; } = null!;
    public ICollection<Match> Matches { get; set; } = [];
}
