namespace FootballBlog.Core.Models;

public class ApiUsageDaily
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string Service { get; set; } = string.Empty; // FootballAPI | Gemini | Telegram
    public int DailyCount { get; set; }
    public int DailyLimit { get; set; } // 0 = unlimited
    public DateTime UpdatedAt { get; set; }
}
