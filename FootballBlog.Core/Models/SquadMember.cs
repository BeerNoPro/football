namespace FootballBlog.Core.Models;

public class SquadMember
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public int PlayerId { get; set; }

    public int? Number { get; set; }
    public string? Position { get; set; }

    public Team? Team { get; set; }
    public Player? Player { get; set; }
}
