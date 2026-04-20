namespace FootballBlog.Core.Models;

public class Standing
{
    public int Id { get; set; }

    public int LeagueId { get; set; }
    public int TeamId { get; set; }
    public int Season { get; set; }

    public int Rank { get; set; }
    public int Points { get; set; }
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int GoalsDiff { get; set; }

    /// <summary>5 trận gần nhất, ví dụ "WWDLW".</summary>
    public string? Form { get; set; }

    /// <summary>Mô tả vị trí, ví dụ "Promotion - Champions League".</summary>
    public string? Description { get; set; }

    /// <summary>Xu hướng: "same" / "up" / "down".</summary>
    public string? Status { get; set; }

    public DateTime UpdatedAt { get; set; }

    public League League { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
