namespace FootballBlog.Core.Models;

public class LiveMatch
{
    public int Id { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    /// <summary>
    /// Trạng thái: SCHEDULED, LIVE, FINISHED, POSTPONED
    /// </summary>
    public string Status { get; set; } = "SCHEDULED";

    public DateTime StartedAt { get; set; }
    public int? Minute { get; set; } // Phút thi đấu, null nếu chưa bắt đầu

    // ID từ Football API bên ngoài
    public int ExternalId { get; set; }

    public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
}
