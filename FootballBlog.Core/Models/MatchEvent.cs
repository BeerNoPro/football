namespace FootballBlog.Core.Models;

public class MatchEvent
{
    public int Id { get; set; }
    public int LiveMatchId { get; set; }
    public LiveMatch LiveMatch { get; set; } = null!;

    public int Minute { get; set; }

    public EventType Type { get; set; }
    public string Description { get; set; } = string.Empty;
}
