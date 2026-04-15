namespace FootballBlog.Core.Models;

public class Match
{
    public int Id { get; set; }

    /// <summary>Fixture ID từ Football API.</summary>
    public int ExternalId { get; set; }

    // FK thay vì strings
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }

    /// <summary>FK → Leagues.Id (internal ID, không phải external API ID).</summary>
    public int LeagueId { get; set; }

    public string Season { get; set; } = string.Empty; // "2024/2025"
    public string? Round { get; set; }                  // "Round 10"

    public DateTime KickoffUtc { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }

    public string? VenueName { get; set; }
    public string? RefereeName { get; set; }

    /// <summary>Thời điểm cuối cùng fetch dữ liệu từ Football API.</summary>
    public DateTime FetchedAt { get; set; }

    // Navigations
    public Team HomeTeam { get; set; } = null!;
    public Team AwayTeam { get; set; } = null!;
    public League League { get; set; } = null!;
    public MatchPrediction? Prediction { get; set; }
    public MatchContextData? ContextData { get; set; }  // AI input — lazy loaded

    /// <summary>LiveMatch tương ứng khi trận đang diễn ra. Null nếu chưa live.</summary>
    public LiveMatch? LiveMatch { get; set; }
}
