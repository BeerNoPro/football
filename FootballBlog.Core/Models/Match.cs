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

    /// <summary>Tỉ số hiệp 1 — null nếu trận chưa qua HT hoặc API không trả về.</summary>
    public int? HtHomeScore { get; set; }
    public int? HtAwayScore { get; set; }

    /// <summary>Tỉ số hiệp phụ (AET) — chỉ có khi status = AET hoặc PEN.</summary>
    public int? EtHomeScore { get; set; }
    public int? EtAwayScore { get; set; }

    /// <summary>Tỉ số loạt penalty — chỉ có khi status = PEN.</summary>
    public int? PenHomeScore { get; set; }
    public int? PenAwayScore { get; set; }

    public string? VenueName { get; set; }
    public string? RefereeName { get; set; }

    /// <summary>Raw JSON từ GET /fixtures/statistics — null cho đến khi FetchPostMatchDataJob chạy sau FT.</summary>
    public string? StatsJson { get; set; }

    /// <summary>Raw JSON từ GET /fixtures/events — null cho đến khi FetchPostMatchDataJob chạy sau FT.</summary>
    public string? EventsJson { get; set; }

    /// <summary>Thời điểm cuối cùng fetch dữ liệu từ Football API.</summary>
    public DateTime FetchedAt { get; set; }

    // Navigations
    public Team HomeTeam { get; set; } = null!;
    public Team AwayTeam { get; set; } = null!;
    public League League { get; set; } = null!;
    public ICollection<MatchPrediction> Predictions { get; set; } = [];
    public MatchContextData? ContextData { get; set; }  // AI input — lazy loaded

    /// <summary>LiveMatch tương ứng khi trận đang diễn ra. Null nếu chưa live.</summary>
    public LiveMatch? LiveMatch { get; set; }
}
