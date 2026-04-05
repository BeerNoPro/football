namespace FootballBlog.Core.Models;

public class MatchEvent
{
    public int Id { get; set; }
    public int LiveMatchId { get; set; }
    public LiveMatch LiveMatch { get; set; } = null!;

    public int Minute { get; set; }

    /// <summary>
    /// Loại sự kiện: GOAL, YELLOW_CARD, RED_CARD, SUBSTITUTION
    /// </summary>
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
