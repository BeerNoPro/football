namespace FootballBlog.Web.Helpers;

/// <summary>Các helper method dùng chung cho PostCard, PostCardCompact, RightSidebar.</summary>
public static class PostHelpers
{
    public static string GetEmoji(string? category) => (category ?? "").ToLower() switch
    {
        var c when c.Contains("dự đoán") || c.Contains("ai") || c.Contains("nhận định") => "🤖",
        var c when c.Contains("phân tích") => "📊",
        var c when c.Contains("tin") => "📰",
        var c when c.Contains("v.league") || c.Contains("vleague") => "🏆",
        _ => "⚽"
    };

    public static string GetThumbBg(string? thumbnail) =>
        string.IsNullOrEmpty(thumbnail)
            ? "linear-gradient(135deg,#1a1a2e,#16213e)"
            : thumbnail;

    public static string FormatTimeAgo(DateTime? dt)
    {
        if (dt is null)
        {
            return "Chưa đăng";
        }

        var diff = DateTime.UtcNow - dt.Value.ToUniversalTime();
        if (diff.TotalMinutes < 1)
        {
            return "Vừa xong";
        }

        if (diff.TotalMinutes < 60)
        {
            return $"{(int)diff.TotalMinutes} phút trước";
        }

        if (diff.TotalHours < 24)
        {
            return $"{(int)diff.TotalHours} giờ trước";
        }

        if (diff.TotalDays < 7)
        {
            return $"{(int)diff.TotalDays} ngày trước";
        }

        return dt.Value.ToString("dd/MM/yyyy");
    }
}
