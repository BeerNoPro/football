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
        if (await dbContext.ApiKeyConfigs.AnyAsync())
        {
            logger.LogDebug("ApiKeyConfigs table already has data — skipping seed");
            return;
        }

        var seeds = new[]
        {
            ("FootballApi", configuration["FootballApi:ApiKey"]),
            ("Claude",      configuration["Claude:ApiKey"]),
            ("Gemini",      configuration["Gemini:ApiKey"]),
        };

        var toInsert = seeds
            .Where(s => !string.IsNullOrWhiteSpace(s.Item2))
            .Select(s => new ApiKeyConfig
            {
                Provider = s.Item1,
                KeyValue = s.Item2!,
                Priority = 1,
                IsActive = true,
                DailyLimit = 0,
                Note = "Seeded from appsettings/user-secrets",
                CreatedAt = DateTime.UtcNow,
            })
            .ToList();

        if (toInsert.Count == 0)
        {
            logger.LogWarning("No API keys found in configuration to seed");
            return;
        }

        dbContext.ApiKeyConfigs.AddRange(toInsert);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} API key(s): {Providers}",
            toInsert.Count,
            string.Join(", ", toInsert.Select(k => k.Provider)));
    }
}
