namespace FootballBlog.Core.Models;

public class Player
{
    public int Id { get; set; }

    /// <summary>Player ID từ api-football.</summary>
    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public string? Nationality { get; set; }
    public string? Position { get; set; }
    public int? Age { get; set; }

    public ICollection<SquadMember> SquadMembers { get; set; } = [];
}
