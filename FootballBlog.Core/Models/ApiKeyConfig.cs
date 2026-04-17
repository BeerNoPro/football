namespace FootballBlog.Core.Models;

public class ApiKeyConfig
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty;   // "FootballApi" | "Claude" | "Gemini"
    public string KeyValue { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int DailyLimit { get; set; }                    // 0 = unlimited
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
