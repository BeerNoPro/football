namespace FootballBlog.Core.Interfaces;

public interface IFootballApiRateLimiter
{
    /// <summary>
    /// Tăng counter request trong ngày hôm nay.
    /// Trả true nếu còn trong giới hạn, false nếu đã vượt quota ngày.
    /// </summary>
    Task<bool> TryConsumeAsync();

    /// <summary>Số request đã dùng hôm nay.</summary>
    Task<int> GetTodayUsageAsync();
}
