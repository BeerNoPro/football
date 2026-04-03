namespace FootballBlog.Core.Models;

public class Match
{
    public int Id { get; set; }

    /// <summary>Fixture ID từ Football API.</summary>
    public int ExternalId { get; set; }

    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int HomeTeamExternalId { get; set; }
    public int AwayTeamExternalId { get; set; }

    public int LeagueId { get; set; }
    public string LeagueName { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string? Round { get; set; }

    public DateTime KickoffUtc { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }

    public string? VenueName { get; set; }
    public string? RefereeName { get; set; }

    /// <summary>Thời điểm cuối cùng fetch dữ liệu từ Football API.</summary>
    public DateTime FetchedAt { get; set; }

    public MatchPrediction? Prediction { get; set; }
}
