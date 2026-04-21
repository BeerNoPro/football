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
    private readonly int _perMinuteLimit = options.Value.PerMinuteRequestLimit;

    private static string TodayKey =>
        $"apikey:usage:FootballApi:global:{DateTime.UtcNow:yyyy-MM-dd}";

    // TTL 65s để align với block time của ApiKeyRotator khi hit 429
    private static string MinuteKey =>
        $"apikey:usage:FootballApi:perminute:{DateTime.UtcNow:yyyy-MM-dd-HH-mm}";

    public async Task<bool> TryConsumeAsync()
    {
        try
        {
            // Per-minute check trước — proactive, tránh hit 429
            long minuteCount = await _db.StringIncrementAsync(MinuteKey);
            if (minuteCount == 1)
            {
                await _db.KeyExpireAsync(MinuteKey, TimeSpan.FromSeconds(65));
            }

            if (minuteCount > _perMinuteLimit)
            {
                await _db.StringDecrementAsync(MinuteKey);
                logger.LogWarning(
                    "Football API per-minute limit reached ({Count}/{Limit}/min). Request blocked.",
                    minuteCount - 1, _perMinuteLimit);
                return false;
            }

            // Daily check sau
            string dailyKey = TodayKey;
            long dailyCount = await _db.StringIncrementAsync(dailyKey);

            if (dailyCount == 1)
            {
                DateTime midnight = DateTime.UtcNow.Date.AddDays(1);
                await _db.KeyExpireAsync(dailyKey, midnight);
            }

            if (dailyCount > _dailyLimit)
            {
                await _db.StringDecrementAsync(dailyKey);
                await _db.StringDecrementAsync(MinuteKey);
                logger.LogWarning(
                    "Football API daily limit reached ({Count}/{Limit}). Request blocked.",
                    dailyCount - 1, _dailyLimit);
                return false;
            }

            logger.LogDebug(
                "Football API request consumed. Usage: {Daily}/{DailyLimit} today, {Minute}/{MinuteLimit}/min",
                dailyCount, _dailyLimit, minuteCount, _perMinuteLimit);
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
