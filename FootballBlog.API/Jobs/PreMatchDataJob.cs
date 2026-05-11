using System.Diagnostics;
using System.Text.Json;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace FootballBlog.API.Jobs;

public class PreMatchDataJob(
    IFootballApiClient apiClient,
    IUnitOfWork uow,
    ILogger<PreMatchDataJob> logger)
{
    /// <summary>
    /// Chạy 5h trước kickoff.
    /// - H2H: 1 API call
    /// - Form + Fatigue + Referee: lấy từ DB, không tốn thêm request
    /// - Sau khi save MatchContextData → enqueue GeneratePredictionJob cho trận này
    /// </summary>
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

        // ── 1. H2H từ API (1 request) ────────────────────────────────────────
        IEnumerable<FixtureRawDto>? h2hFixtures = await apiClient.GetHeadToHeadAsync(homeTeamExternalId, awayTeamExternalId);
        if (h2hFixtures is null)
        {
            logger.LogWarning("H2H fetch returned null for fixture {FixtureId} — rate limit or no key", fixtureExternalId);
            return;
        }

        // ── 2. Form + Fatigue từ DB (0 request) ──────────────────────────────
        List<Match> homeRecent = (await uow.Matches.GetRecentByTeamAsync(match.HomeTeamId, 5)).ToList();
        List<Match> awayRecent = (await uow.Matches.GetRecentByTeamAsync(match.AwayTeamId, 5)).ToList();

        // ── 3. Build MatchContext ─────────────────────────────────────────────
        List<FixtureRawDto> h2hList = h2hFixtures.OrderByDescending(f => f.KickoffUtc).Take(10).ToList();

        int homeWins = h2hList.Count(f =>
            (f.HomeTeamExternalId == homeTeamExternalId && f.HomeScore > f.AwayScore) ||
            (f.AwayTeamExternalId == homeTeamExternalId && f.AwayScore > f.HomeScore));
        int awayWins = h2hList.Count(f =>
            (f.HomeTeamExternalId == awayTeamExternalId && f.HomeScore > f.AwayScore) ||
            (f.AwayTeamExternalId == awayTeamExternalId && f.AwayScore > f.HomeScore));
        int draws = h2hList.Count(f => f.HomeScore == f.AwayScore);

        MatchContext context = new()
        {
            H2H = new H2HContext
            {
                RecentMatches = h2hList.Select(f => new H2HMatch
                {
                    Date = f.KickoffUtc,
                    HomeTeam = f.HomeTeamName,
                    AwayTeam = f.AwayTeamName,
                    HomeScore = f.HomeScore ?? 0,
                    AwayScore = f.AwayScore ?? 0,
                    Competition = f.LeagueName
                }).ToList(),
                HomeWins = homeWins,
                Draws = draws,
                AwayWins = awayWins
            },
            HomeForm = BuildTeamForm(homeRecent, match.HomeTeamId),
            AwayForm = BuildTeamForm(awayRecent, match.AwayTeamId),
            Referee = string.IsNullOrEmpty(match.RefereeName)
                ? null
                : new RefereeContext { Name = match.RefereeName },
            Fatigue = BuildFatigue(homeRecent, awayRecent, match.KickoffUtc),
            Lineup = null // lineup chỉ có 15min trước — không dùng FetchLineupsAsync nữa để tiết kiệm request
        };

        // ── 4. Upsert MatchContextData ────────────────────────────────────────
        string contextJson = JsonSerializer.Serialize(context);
        MatchContextData? existing = await uow.MatchContexts.GetByMatchIdAsync(match.Id);

        if (existing is null)
        {
            await uow.MatchContexts.AddAsync(new MatchContextData
            {
                MatchId = match.Id,
                ContextJson = contextJson,
                FetchedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.ContextJson = contextJson;
            existing.FetchedAt = DateTime.UtcNow;
            await uow.MatchContexts.UpdateAsync(existing);
        }

        await uow.CommitAsync();

        sw.Stop();
        logger.LogInformation(
            "PreMatchDataJob.FetchH2H finished for fixture {FixtureId}. H2H={H2H}, HomeForm={HF}, AwayForm={AF}, Duration={DurationMs}ms",
            fixtureExternalId, h2hList.Count, homeRecent.Count, awayRecent.Count, sw.ElapsedMilliseconds);
    }

    /// <summary>Không dùng để tiết kiệm request — lineup bỏ qua, Gemini vẫn đủ data từ H2H + form.</summary>
    public Task FetchLineupsAsync(int fixtureExternalId)
    {
        logger.LogDebug("FetchLineupsAsync skipped for fixture {FixtureId} — conserving API quota", fixtureExternalId);
        return Task.CompletedTask;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TeamFormContext BuildTeamForm(List<Match> matches, int teamId)
    {
        if (matches.Count == 0)
        {
            return new TeamFormContext();
        }

        Match first = matches[0];
        string teamName = first.HomeTeamId == teamId
            ? first.HomeTeam?.Name ?? string.Empty
            : first.AwayTeam?.Name ?? string.Empty;

        List<FormMatch> formMatches = matches.Select(m =>
        {
            bool isHome = m.HomeTeamId == teamId;
            int goalsFor = isHome ? (m.HomeScore ?? 0) : (m.AwayScore ?? 0);
            int goalsAgainst = isHome ? (m.AwayScore ?? 0) : (m.HomeScore ?? 0);
            string result = goalsFor > goalsAgainst ? "W" : goalsFor < goalsAgainst ? "L" : "D";

            return new FormMatch
            {
                Date = m.KickoffUtc,
                Opponent = isHome ? m.AwayTeam?.Name ?? string.Empty : m.HomeTeam?.Name ?? string.Empty,
                IsHome = isHome,
                GoalsFor = goalsFor,
                GoalsAgainst = goalsAgainst,
                Result = result,
                Competition = m.League?.Name ?? string.Empty
            };
        }).ToList();

        return new TeamFormContext
        {
            TeamName = teamName,
            RecentMatches = formMatches,
            FormString = string.Join("", formMatches.Select(f => f.Result)),
            GoalsScored = formMatches.Sum(f => f.GoalsFor),
            GoalsConceded = formMatches.Sum(f => f.GoalsAgainst)
        };
    }

    private static FatigueContext BuildFatigue(List<Match> homeMatches, List<Match> awayMatches, DateTime kickoffUtc)
    {
        DateTime? homeLastMatch = homeMatches.OrderByDescending(m => m.KickoffUtc).FirstOrDefault()?.KickoffUtc;
        DateTime? awayLastMatch = awayMatches.OrderByDescending(m => m.KickoffUtc).FirstOrDefault()?.KickoffUtc;

        return new FatigueContext
        {
            HomeDaysSinceLastMatch = homeLastMatch.HasValue
                ? (int)(kickoffUtc - homeLastMatch.Value).TotalDays
                : null,
            AwayDaysSinceLastMatch = awayLastMatch.HasValue
                ? (int)(kickoffUtc - awayLastMatch.Value).TotalDays
                : null,
            HomePlayingEurope = false,
            AwayPlayingEurope = false
        };
    }
}
