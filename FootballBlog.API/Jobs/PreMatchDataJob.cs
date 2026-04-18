using System.Diagnostics;
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
        var sw = Stopwatch.StartNew();
        logger.LogInformation(
            "PreMatchDataJob.FetchH2H started for fixture {FixtureId} ({HomeId} vs {AwayId})",
            fixtureExternalId, homeTeamExternalId, awayTeamExternalId);

        Match? match = await uow.Matches.GetByExternalIdAsync(fixtureExternalId);
        if (match is null)
        {
            logger.LogWarning("Fixture {FixtureId} not found in DB", fixtureExternalId);
            return;
        }

        IEnumerable<FixtureRawDto>? h2hFixtures = await apiClient.GetHeadToHeadAsync(homeTeamExternalId, awayTeamExternalId);
        if (h2hFixtures is null)
        {
            logger.LogWarning("H2H fetch returned null for fixture {FixtureId} (rate limit or HTTP error)", fixtureExternalId);
            return;
        }

        sw.Stop();
        // Phase 5 sẽ persist H2H data vào MatchContextData.ContextJson
        // Hiện tại chỉ log để xác nhận data có sẵn
        logger.LogInformation(
            "PreMatchDataJob.FetchH2H finished for fixture {FixtureId}. HistoricalMatches={Count}, Duration={DurationMs}ms",
            fixtureExternalId, h2hFixtures.Count(), sw.ElapsedMilliseconds);
    }

    /// <summary>Chạy 15min trước kickoff. Fetch confirmed lineups để chuẩn bị context cho AI Prediction (Phase 5).</summary>
    public async Task FetchLineupsAsync(int fixtureExternalId)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation(
            "PreMatchDataJob.FetchLineups started for fixture {FixtureId}", fixtureExternalId);

        Match? match = await uow.Matches.GetByExternalIdAsync(fixtureExternalId);
        if (match is null)
        {
            logger.LogWarning("Fixture {FixtureId} not found in DB", fixtureExternalId);
            return;
        }

        string? lineupsJson = await apiClient.GetLineupsRawAsync(fixtureExternalId);
        if (lineupsJson is null)
        {
            logger.LogWarning("Lineup fetch returned null for fixture {FixtureId} (rate limit or HTTP error)", fixtureExternalId);
            return;
        }

        sw.Stop();
        // Phase 5 sẽ lưu vào MatchContextData.ContextJson
        logger.LogInformation(
            "PreMatchDataJob.FetchLineups finished for fixture {FixtureId}. JsonLength={Length} chars, Duration={DurationMs}ms",
            fixtureExternalId, lineupsJson.Length, sw.ElapsedMilliseconds);
    }
}
