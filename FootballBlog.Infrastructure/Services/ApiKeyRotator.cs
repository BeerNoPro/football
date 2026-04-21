using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FootballBlog.Infrastructure.Services;

public class ApiKeyRotator(
    ApplicationDbContext dbContext,
    IConnectionMultiplexer redis,
    ILogger<ApiKeyRotator> logger) : IApiKeyRotator
{
    private readonly IDatabase _db = redis.GetDatabase();
    private static readonly TimeSpan ListCacheTtl = TimeSpan.FromMinutes(5);

    private static string ListCacheKey(string provider) => $"apikey:list:{provider}";

    private static string UsageKey(string provider, string keyHash)
        => $"apikey:usage:{provider}:{keyHash}:{DateTime.UtcNow:yyyy-MM-dd}";

    // Key riêng để block key ngay lập tức khi nhận 429/403 — bất kể DailyLimit
    private static string BlockedKey(string provider, string keyHash)
        => $"apikey:blocked:{provider}:{keyHash}:{DateTime.UtcNow:yyyy-MM-dd}";

    private static string KeyHash(string key)
        => Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(key)))[..8].ToLower();

    public async Task<string?> GetAvailableKeyAsync(string provider)
    {
        try
        {
            var keys = await GetKeysFromCacheOrDbAsync(provider);

            if (keys.Count == 0)
            {
                logger.LogWarning("No API keys configured for provider {Provider} — add keys via Admin UI", provider);
                return null;
            }

            int blockedCount = 0;
            int exhaustedCount = 0;

            foreach (var key in keys)
            {
                string hash = KeyHash(key.KeyValue);

                if (await _db.KeyExistsAsync(BlockedKey(provider, hash)))
                {
                    TimeSpan? ttl = await _db.KeyTimeToLiveAsync(BlockedKey(provider, hash));
                    logger.LogDebug("API key {Hash} for {Provider} blocked — expires in {Ttl}", hash, provider, ttl);
                    blockedCount++;
                    continue;
                }

                if (key.DailyLimit > 0)
                {
                    RedisValue usageVal = await _db.StringGetAsync(UsageKey(provider, hash));
                    int used = usageVal.HasValue ? (int)usageVal : 0;
                    if (used >= key.DailyLimit)
                    {
                        logger.LogDebug("API key {Hash} for {Provider} daily limit exhausted ({Used}/{Limit})", hash, provider, used, key.DailyLimit);
                        exhaustedCount++;
                        continue;
                    }
                }

                return key.KeyValue;
            }

            logger.LogWarning(
                "No available API key for {Provider} — {Blocked} blocked, {Exhausted} daily-exhausted out of {Total} keys. Jobs will resume after Redis TTL expires.",
                provider, blockedCount, exhaustedCount, keys.Count);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting available key for {Provider}", provider);
            return null;
        }
    }

    public async Task MarkExhaustedAsync(string provider, string key, bool isDailyLimit = true)
    {
        try
        {
            string hash = KeyHash(key);
            TimeSpan ttl = isDailyLimit
                ? DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow
                : TimeSpan.FromSeconds(65);

            await _db.StringSetAsync(BlockedKey(provider, hash), 1, ttl);

            if (isDailyLimit)
            {
                logger.LogWarning(
                    "API key {Hash} for {Provider} DAILY LIMIT exhausted — blocked until midnight UTC ({Ttl:hh\\:mm\\:ss} remaining)",
                    hash, provider, ttl);
            }
            else
            {
                logger.LogWarning(
                    "API key {Hash} for {Provider} per-minute rate limit hit — blocked for 65s",
                    hash, provider);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to mark key exhausted for {Provider}", provider);
        }
    }

    private async Task<List<ApiKeyConfig>> GetKeysFromCacheOrDbAsync(string provider)
    {
        string cacheKey = ListCacheKey(provider);

        try
        {
            RedisValue cached = await _db.StringGetAsync(cacheKey);
            if (cached.HasValue)
            {
                var list = JsonSerializer.Deserialize<List<ApiKeyConfig>>(cached.ToString());
                if (list is not null)
                {
                    return list;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache miss for API keys list {Provider}", provider);
        }

        var keys = await dbContext.ApiKeyConfigs
            .AsNoTracking()
            .Where(k => k.Provider == provider && k.IsActive)
            .OrderBy(k => k.Priority)
            .ToListAsync();

        try
        {
            string json = JsonSerializer.Serialize(keys);
            await _db.StringSetAsync(cacheKey, json, ListCacheTtl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to cache API keys list for {Provider}", provider);
        }

        return keys;
    }

    /// <summary>Xóa cache list để force reload từ DB (gọi sau khi thêm/xóa/toggle key)</summary>
    public async Task InvalidateCacheAsync(string provider)
    {
        try
        {
            await _db.KeyDeleteAsync(ListCacheKey(provider));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to invalidate cache for {Provider}", provider);
        }
    }
}
