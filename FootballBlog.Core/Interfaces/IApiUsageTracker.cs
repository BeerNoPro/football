namespace FootballBlog.Core.Interfaces;

public interface IApiUsageTracker
{
    /// <summary>Kiểm tra service còn quota hôm nay không. DailyLimit=0 = unlimited.</summary>
    Task<bool> CanCallAsync(string service);

    /// <summary>Ghi nhận 1 API call thành công vào DB.</summary>
    Task IncrementAsync(string service);

    /// <summary>Lấy usage hôm nay của service.</summary>
    Task<(int Count, int Limit)> GetTodayAsync(string service);
}
