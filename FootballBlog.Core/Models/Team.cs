namespace FootballBlog.Core.Models;

public class Team
{
    public int Id { get; set; }

    /// <summary>Team ID từ api-football.</summary>
    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;   // "Manchester United"
    public string? ShortName { get; set; }              // "Man Utd"
    public string? LogoUrl { get; set; }

    /// <summary>Nullable — club có thể không gắn với quốc gia cụ thể (ví dụ: đội tuyển quốc tế).</summary>
    public int? CountryId { get; set; }

    /// <summary>Sân vận động chủ sân — populate từ /teams endpoint.</summary>
    public int? VenueId { get; set; }

    public Country? Country { get; set; }
    public Venue? Venue { get; set; }
    public ICollection<Match> HomeMatches { get; set; } = [];
    public ICollection<Match> AwayMatches { get; set; } = [];
    public ICollection<SquadMember> SquadMembers { get; set; } = [];
}
