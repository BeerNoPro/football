using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FootballBlog.Infrastructure.Services;

public class RedisFootballApiRateLimiter(
    IConnectionMultiplexer redis,
    IOptions<FootballApiOptions> options,
    ILogger<RedisFootballApiRateLimiter> logger) : IFootballApiRateLimiter
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly int _dailyLimit = options.Value.DailyRequestLimit;

    private static string TodayKey => $"football_api:requests:{DateTime.UtcNow:yyyy-MM-dd}";

    public async Task<bool> TryConsumeAsync()
    {
        try
        {
            string key = TodayKey;
            long newCount = await _db.StringIncrementAsync(key);

            // Lần đầu tiên trong ngày — set expire đến midnight UTC ngày mai
            if (newCount == 1)
            {
                DateTime midnight = DateTime.UtcNow.Date.AddDays(1);
                await _db.KeyExpireAsync(key, midnight);
            }

            if (newCount > _dailyLimit)
            {
                await _db.StringDecrementAsync(key); // hoàn lại
                logger.LogWarning(
                    "Football API daily limit reached ({Count}/{Limit}). Request blocked.",
                    newCount - 1, _dailyLimit);
                return false;
            }

            logger.LogDebug("Football API request consumed. Usage: {Count}/{Limit}", newCount, _dailyLimit);
            return true;
        }
        catch (Exception ex)
        {
            // Fail open — Redis down không được block jobs
            logger.LogWarning(ex, "Redis unavailable for rate limiting. Allowing request.");
            return true;
        }
    }

    public async Task<int> GetTodayUsageAsync()
    {
        try
        {
            RedisValue value = await _db.StringGetAsync(TodayKey);
            return value.HasValue ? (int)value : 0;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis unavailable. Cannot get today's usage.");
            return 0;
        }
    }
}
