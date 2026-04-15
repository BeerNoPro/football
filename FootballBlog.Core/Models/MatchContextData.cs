namespace FootballBlog.Core.Models;

/// <summary>
/// Entity lưu context data cho AI prediction — tách ra để không load khi chỉ cần match list.
/// 1-to-1 với Match, lazy loaded.
/// </summary>
public class MatchContextData
{
    public int Id { get; set; }
    public int MatchId { get; set; }

    /// <summary>JSONB blob chứa toàn bộ context cho AI prediction (H2H, form, lineup, fatigue).</summary>
    public string ContextJson { get; set; } = "{}";

    public DateTime FetchedAt { get; set; }

    public Match Match { get; set; } = null!;
}
