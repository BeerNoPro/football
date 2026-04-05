namespace FootballBlog.Core.Models;

public class LiveMatch
{
    public int Id { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    public DateTime StartedAt { get; set; }
    public int? Minute { get; set; } // Phút thi đấu, null nếu chưa bắt đầu

    // ID từ Football API bên ngoài
    public int ExternalId { get; set; }

    /// <summary>FK sang Match (scheduling + prediction). Null nếu chưa được fetch bởi FetchUpcomingMatchesJob.</summary>
    public int? MatchId { get; set; }
    public Match? Match { get; set; }

    public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
}
