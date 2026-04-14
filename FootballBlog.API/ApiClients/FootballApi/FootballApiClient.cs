using System.Text.Json;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FootballBlog.API.ApiClients.FootballApi;

public class FootballApiClient(
    HttpClient httpClient,
    IOptions<FootballApiOptions> options,
    IFootballApiRateLimiter rateLimiter,
    ILogger<FootballApiClient> logger) : IFootballApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<Match>?> GetUpcomingFixturesAsync(int leagueId, int next = 20)
    {
        if (!await rateLimiter.TryConsumeAsync())
        {
            return null;
        }

        try
        {
            logger.LogDebug("Fetching upcoming fixtures for league {LeagueId}, next={Next}", leagueId, next);

            string season = DateTime.UtcNow.Month >= 7
                ? DateTime.UtcNow.Year.ToString()
                : (DateTime.UtcNow.Year - 1).ToString();

            var envelope = await httpClient.GetFromJsonAsync<FootballApiEnvelope<FixtureResponse>>(
                $"fixtures?league={leagueId}&season={season}&next={next}", JsonOptions);

            if (envelope?.Response is null)
            {
                logger.LogWarning("Empty response for league {LeagueId}", leagueId);
                return null;
            }

            IEnumerable<Match> matches = envelope.Response.Select(MapToMatch);
            logger.LogInformation("Fetched {Count} upcoming fixtures for league {LeagueId}", matches.Count(), leagueId);
            return matches;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch upcoming fixtures for league {LeagueId}", leagueId);
            return null;
        }
    }

    public async Task<IEnumerable<LiveMatch>?> GetAllLiveFixturesAsync()
    {
        if (!await rateLimiter.TryConsumeAsync())
        {
            return null;
        }

        try
        {
            logger.LogDebug("Fetching all live fixtures");

            var envelope = await httpClient.GetFromJsonAsync<FootballApiEnvelope<FixtureResponse>>(
                "fixtures?live=all", JsonOptions);

            if (envelope?.Response is null)
            {
                return [];
            }

            IEnumerable<LiveMatch> liveMatches = envelope.Response.Select(MapToLiveMatch);
            logger.LogInformation("Fetched {Count} live fixtures", liveMatches.Count());
            return liveMatches;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch live fixtures");
            return null;
        }
    }

    public async Task<IEnumerable<Match>?> GetHeadToHeadAsync(int homeTeamId, int awayTeamId, int last = 10)
    {
        if (!await rateLimiter.TryConsumeAsync())
        {
            return null;
        }

        try
        {
            logger.LogDebug("Fetching H2H for {Home} vs {Away}", homeTeamId, awayTeamId);

            var envelope = await httpClient.GetFromJsonAsync<FootballApiEnvelope<FixtureResponse>>(
                $"fixtures/headtohead?h2h={homeTeamId}-{awayTeamId}&last={last}", JsonOptions);

            if (envelope?.Response is null)
            {
                return [];
            }

            IEnumerable<Match> matches = envelope.Response.Select(MapToMatch);
            logger.LogInformation("Fetched {Count} H2H matches for {Home} vs {Away}", matches.Count(), homeTeamId, awayTeamId);
            return matches;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch H2H for {Home} vs {Away}", homeTeamId, awayTeamId);
            return null;
        }
    }

    public async Task<string?> GetLineupsRawAsync(int fixtureId)
    {
        if (!await rateLimiter.TryConsumeAsync())
        {
            return null;
        }

        try
        {
            logger.LogDebug("Fetching lineups for fixture {FixtureId}", fixtureId);

            HttpResponseMessage response = await httpClient.GetAsync($"fixtures/lineups?fixture={fixtureId}");
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            logger.LogInformation("Fetched lineups for fixture {FixtureId}", fixtureId);
            return json;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch lineups for fixture {FixtureId}", fixtureId);
            return null;
        }
    }

    // ── Mappers ────────────────────────────────────────────────────────────────

    private static Match MapToMatch(FixtureResponse r) => new()
    {
        ExternalId = r.Fixture.Id,
        HomeTeam = r.Teams.Home.Name,
        AwayTeam = r.Teams.Away.Name,
        HomeTeamExternalId = r.Teams.Home.Id,
        AwayTeamExternalId = r.Teams.Away.Id,
        LeagueId = r.League.Id,
        LeagueName = r.League.Name,
        Season = r.League.Season.ToString(),
        Round = r.League.Round,
        KickoffUtc = r.Fixture.Date,
        Status = MapStatus(r.Fixture.Status.Short),
        HomeScore = r.Goals.Home,
        AwayScore = r.Goals.Away,
        VenueName = r.Fixture.Venue?.Name,
        RefereeName = r.Fixture.Referee,
        FetchedAt = DateTime.UtcNow
    };

    private static LiveMatch MapToLiveMatch(FixtureResponse r) => new()
    {
        ExternalId = r.Fixture.Id,
        HomeTeam = r.Teams.Home.Name,
        AwayTeam = r.Teams.Away.Name,
        HomeScore = r.Goals.Home ?? 0,
        AwayScore = r.Goals.Away ?? 0,
        Status = MapStatus(r.Fixture.Status.Short),
        Minute = r.Fixture.Status.Elapsed,
        StartedAt = r.Fixture.Date
    };

    private static MatchStatus MapStatus(string s) => s switch
    {
        "NS" => MatchStatus.Scheduled,
        "1H" or "2H" or "HT" or "ET" or "P" or "LIVE" or "BT" => MatchStatus.Live,
        "FT" or "AET" or "PEN" => MatchStatus.Finished,
        "PST" => MatchStatus.Postponed,
        "SUSP" or "CANC" or "ABD" or "WO" => MatchStatus.Cancelled,
        _ => MatchStatus.Scheduled
    };
}
