using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Logging;

namespace FootballBlog.API.Jobs;

public class PreMatchDataJob(
    IFootballApiClient apiClient,
    IUnitOfWork uow,
    ILogger<PreMatchDataJob> logger)
{
    /// <summary>Chạy 5h trước kickoff. Fetch H2H để chuẩn bị context cho AI Prediction (Phase 5).</summary>
    public async Task FetchH2HAsync(int fixtureExternalId, int homeTeamExternalId, int awayTeamExternalId)
    {
        logger.LogInformation(
            "PreMatchDataJob FetchH2H started for fixture {FixtureId} ({Home} vs {Away})",
            fixtureExternalId, homeTeamExternalId, awayTeamExternalId);

        Match? match = await uow.Matches.GetByExternalIdAsync(fixtureExternalId);
        if (match is null)
        {
            logger.LogWarning("Fixture {FixtureId} not found in DB. Skipping H2H fetch.", fixtureExternalId);
            return;
        }

        IEnumerable<FixtureRawDto>? h2hFixtures = await apiClient.GetHeadToHeadAsync(homeTeamExternalId, awayTeamExternalId);
        if (h2hFixtures is null)
        {
            logger.LogWarning("H2H fetch returned null for fixture {FixtureId} (rate limit or HTTP error)", fixtureExternalId);
            return;
        }

        // Phase 5 sẽ persist H2H data vào MatchContextData.ContextJson
        // Hiện tại chỉ log để xác nhận data có sẵn
        logger.LogInformation(
            "H2H fetch complete for fixture {FixtureId}. Found {Count} historical matches.",
            fixtureExternalId, h2hFixtures.Count());
    }

    /// <summary>Chạy 15min trước kickoff. Fetch confirmed lineups để chuẩn bị context cho AI Prediction (Phase 5).</summary>
    public async Task FetchLineupsAsync(int fixtureExternalId)
    {
        logger.LogInformation(
            "PreMatchDataJob FetchLineups started for fixture {FixtureId}", fixtureExternalId);

        Match? match = await uow.Matches.GetByExternalIdAsync(fixtureExternalId);
        if (match is null)
        {
            logger.LogWarning("Fixture {FixtureId} not found in DB. Skipping lineup fetch.", fixtureExternalId);
            return;
        }

        string? lineupsJson = await apiClient.GetLineupsRawAsync(fixtureExternalId);
        if (lineupsJson is null)
        {
            logger.LogWarning("Lineup fetch returned null for fixture {FixtureId} (rate limit or HTTP error)", fixtureExternalId);
            return;
        }

        // Phase 5 sẽ lưu vào MatchContextData.ContextJson
        logger.LogInformation(
            "Lineup fetch complete for fixture {FixtureId}. JSON length: {Length} chars.",
            fixtureExternalId, lineupsJson.Length);
    }
}
