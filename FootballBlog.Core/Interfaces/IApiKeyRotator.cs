namespace FootballBlog.Core.Interfaces;

public interface IApiKeyRotator
{
    Task<string?> GetAvailableKeyAsync(string provider);
    Task MarkExhaustedAsync(string provider, string key, bool isDailyLimit = true);
    Task InvalidateCacheAsync(string provider);
}
