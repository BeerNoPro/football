using System.Diagnostics;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FootballBlog.API.Jobs;

/// <summary>
/// Fetch statistics + events sau khi trận kết thúc (FT/AET/PEN).
/// Lưu raw JSON vào Match.StatsJson / EventsJson để phân tích offline.
/// Chỉ chạy cho premium leagues — tiết kiệm quota (2 req/trận).
/// Trigger: tự động từ FetchUpcomingMatchesJob (daily) + LiveScorePollingJob (khi FT) + Admin UI.
/// </summary>
public class FetchPostMatchDataJob(
    IFootballApiClient apiClient,
    IUnitOfWork uow,
    IOptions<FootballApiOptions> options,
    ILogger<FetchPostMatchDataJob> logger)
{
    public async Task ExecuteAsync()
    {
        var sw = Stopwatch.StartNew();
        var premiumLeagues = new HashSet<int>(options.Value.PremiumLeagueIds);

        var pending = await uow.Matches.GetFinishedWithoutStatsAsync(limit: 15);
        var targets = pending
            .Where(m => premiumLeagues.Contains(m.League.ExternalId))
            .ToList();

        logger.LogInformation("FetchPostMatchDataJob started. Pending without stats (premium): {Count}", targets.Count);

        int fetched = 0;
        int skipped = 0;

        foreach (var match in targets)
        {
            var (statsJson, eventsJson) = await apiClient.GetFixturePostMatchDataAsync(match.ExternalId);

            if (statsJson is null && eventsJson is null)
            {
                logger.LogWarning("Quota hit or error for fixture {ExternalId} — aborting run", match.ExternalId);
                break;
            }

            // Reload tracked entity để update
            var tracked = await uow.Matches.GetByExternalIdAsync(match.ExternalId);
            if (tracked is null)
            {
                skipped++;
                continue;
            }

            tracked.StatsJson = statsJson;
            tracked.EventsJson = eventsJson;
            await uow.Matches.UpdateAsync(tracked);
            await uow.CommitAsync();

            fetched++;
            logger.LogInformation(
                "PostMatchData saved for {Home} vs {Away} (fixture {ExternalId}): stats={HasStats}, events={HasEvents}",
                match.HomeTeam?.Name, match.AwayTeam?.Name, match.ExternalId,
                statsJson is not null, eventsJson is not null);
        }

        sw.Stop();
        logger.LogInformation(
            "FetchPostMatchDataJob finished. Fetched={Fetched}, Skipped={Skipped}, Duration={DurationMs}ms",
            fetched, skipped, sw.ElapsedMilliseconds);
    }
}
