namespace FootballBlog.Core.Models;

/// <summary>
/// POCO dùng để serialize/deserialize ContextJson trong MatchContextData.
/// Không phải EF entity — không có DbSet.
/// </summary>
public class MatchContext
{
    public H2HContext H2H { get; set; } = new();
    public TeamFormContext HomeForm { get; set; } = new();
    public TeamFormContext AwayForm { get; set; } = new();
    public LineupContext? Lineup { get; set; }
    public RefereeContext? Referee { get; set; }
    public FatigueContext? Fatigue { get; set; }
}

public class H2HContext
{
    public List<H2HMatch> RecentMatches { get; set; } = [];    // 5 trận gần nhất
    public int HomeWins { get; set; }
    public int Draws { get; set; }
    public int AwayWins { get; set; }
}

public class H2HMatch
{
    public DateTime Date { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public string Competition { get; set; } = string.Empty;
}

public class TeamFormContext
{
    public string TeamName { get; set; } = string.Empty;
    public List<FormMatch> RecentMatches { get; set; } = [];   // 5 trận gần nhất
    public string FormString { get; set; } = string.Empty;     // "WWDLW"
    public int GoalsScored { get; set; }
    public int GoalsConceded { get; set; }
}

public class FormMatch
{
    public DateTime Date { get; set; }
    public string Opponent { get; set; } = string.Empty;
    public bool IsHome { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public string Result { get; set; } = string.Empty;         // "W", "D", "L"
    public string Competition { get; set; } = string.Empty;
}

public class LineupContext
{
    public List<string> HomeProbableXI { get; set; } = [];
    public List<string> AwayProbableXI { get; set; } = [];
    public List<string> HomeInjuries { get; set; } = [];       // treo giò / chấn thương
    public List<string> AwayInjuries { get; set; } = [];
}

public class RefereeContext
{
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }                         // "strict referee, 4.2 yellows/game"
}

public class FatigueContext
{
    public int? HomeDaysSinceLastMatch { get; set; }
    public int? AwayDaysSinceLastMatch { get; set; }
    public bool HomePlayingEurope { get; set; }
    public bool AwayPlayingEurope { get; set; }
    public string? Notes { get; set; }                         // "Home played Thu Europa League"
}
