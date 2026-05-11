using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FootballBlog.Infrastructure.Services;

public class ApiKeySeeder(
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    ILogger<ApiKeySeeder> logger)
{
    public async Task SeedAsync()
    {
        var seeds = new[]
        {
            ("FootballApi", configuration["FootballApi:ApiKey"]),
            ("Claude",      configuration["Claude:ApiKey"]),
            ("Gemini",      configuration["Gemini:ApiKey"]),
        };

        var toInsert = new List<ApiKeyConfig>();

        foreach (var (provider, value) in seeds.Where(s => !string.IsNullOrWhiteSpace(s.Item2)))
        {
            bool exists = await dbContext.ApiKeyConfigs.AnyAsync(k => k.Provider == provider);
            if (exists)
            {
                logger.LogDebug("ApiKey for {Provider} already seeded — skipping", provider);
                continue;
            }

            toInsert.Add(new ApiKeyConfig
            {
                Provider = provider,
                KeyValue = value!,
                Priority = 1,
                IsActive = true,
                DailyLimit = 0,
                Note = "Seeded from appsettings/user-secrets",
                CreatedAt = DateTime.UtcNow,
            });
        }

        if (toInsert.Count == 0)
        {
            logger.LogDebug("All API keys already seeded — nothing to do");
            return;
        }

        dbContext.ApiKeyConfigs.AddRange(toInsert);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} API key(s): {Providers}",
            toInsert.Count,
            string.Join(", ", toInsert.Select(k => k.Provider)));
    }
}
