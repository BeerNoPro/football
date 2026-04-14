using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Core.Options;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FootballBlog.API.Jobs;

public class FetchUpcomingMatchesJob(
    IFootballApiClient apiClient,
    IUnitOfWork uow,
    IOptions<FootballApiOptions> options,
    ILogger<FetchUpcomingMatchesJob> logger)
{
    public async Task ExecuteAsync()
    {
        FootballApiOptions opts = options.Value;
        logger.LogInformation("FetchUpcomingMatchesJob started. Leagues: {Count}", opts.LeagueIds.Length);

        int newMatches = 0;
        int updatedMatches = 0;
        DateTime now = DateTime.UtcNow;

        foreach (int leagueId in opts.LeagueIds)
        {
            IEnumerable<Match>? fixtures = await apiClient.GetUpcomingFixturesAsync(leagueId, opts.FixturesPerLeague);
            if (fixtures is null)
            {
                logger.LogWarning("Skipping league {LeagueId} — null response (rate limit or HTTP error)", leagueId);
                continue;
            }

            foreach (Match fixture in fixtures)
            {
                Match? existing = await uow.Matches.GetByExternalIdAsync(fixture.ExternalId);

                if (existing is null)
                {
                    await uow.Matches.AddAsync(fixture);
                    newMatches++;

                    // Schedule pre-match data jobs — chỉ khi thời gian còn trong tương lai
                    DateTime h2hTime = fixture.KickoffUtc.AddHours(-5);
                    DateTime lineupTime = fixture.KickoffUtc.AddMinutes(-15);

                    if (h2hTime > now)
                    {
                        BackgroundJob.Schedule<PreMatchDataJob>(
                            j => j.FetchH2HAsync(fixture.ExternalId, fixture.HomeTeamExternalId, fixture.AwayTeamExternalId),
                            h2hTime);

                        logger.LogDebug(
                            "Scheduled H2H fetch for fixture {FixtureId} at {Time}",
                            fixture.ExternalId, h2hTime);
                    }

                    if (lineupTime > now)
                    {
                        BackgroundJob.Schedule<PreMatchDataJob>(
                            j => j.FetchLineupsAsync(fixture.ExternalId),
                            lineupTime);

                        logger.LogDebug(
                            "Scheduled lineup fetch for fixture {FixtureId} at {Time}",
                            fixture.ExternalId, lineupTime);
                    }
                }
                else
                {
                    // Idempotent update — chỉ cập nhật status + score
                    existing.Status = fixture.Status;
                    existing.HomeScore = fixture.HomeScore;
                    existing.AwayScore = fixture.AwayScore;
                    existing.FetchedAt = now;
                    await uow.Matches.UpdateAsync(existing);
                    updatedMatches++;
                }
            }
        }

        await uow.CommitAsync();

        logger.LogInformation(
            "FetchUpcomingMatchesJob finished. New={New}, Updated={Updated}",
            newMatches, updatedMatches);
    }
}
