namespace FootballBlog.Core.Models;

public class MatchPrediction
{
    public int Id { get; set; }
    public int MatchId { get; set; }

    /// <summary>"Claude" hoặc "Gemini"</summary>
    public string AIProvider { get; set; } = string.Empty;

    /// <summary>Model cụ thể, ví dụ "claude-opus-4-6"</summary>
    public string AIModel { get; set; } = string.Empty;

    public int? PredictedHomeScore { get; set; }
    public int? PredictedAwayScore { get; set; }

    /// <summary>"HomeWin" | "Draw" | "AwayWin"</summary>
    public string PredictedOutcome { get; set; } = string.Empty;

    /// <summary>Độ tự tin 0–100.</summary>
    public decimal ConfidenceScore { get; set; }

    /// <summary>Phân tích đầy đủ dạng markdown từ AI.</summary>
    public string AnalysisSummary { get; set; } = string.Empty;

    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }

    public PredictionPhase Phase { get; set; } = PredictionPhase.PreMatch;

    /// <summary>Raw response từ AI trước khi parse — dùng để audit và tune prompt.</summary>
    public string? RawResponse { get; set; }

    public DateTime GeneratedAt { get; set; }

    /// <summary>Message ID trên Telegram — PreMatch dùng để edit khi HT.</summary>
    public long? TelegramMessageId { get; set; }

    // Navigation
    public Match Match { get; set; } = null!;
}
