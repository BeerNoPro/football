using System.Text.Json;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Core.Options;
using Microsoft.Extensions.Logging;

namespace FootballBlog.API.ApiClients.FootballApi;

public class FootballApiClient(
    HttpClient httpClient,
    IFootballApiRateLimiter rateLimiter,
    ILogger<FootballApiClient> logger) : IFootballApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<FixtureRawDto>?> GetUpcomingFixturesAsync(int leagueId, int next = 20)
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

            IEnumerable<FixtureRawDto> fixtures = envelope.Response.Select(MapToFixtureDto);
            logger.LogInformation("Fetched {Count} upcoming fixtures for league {LeagueId}", fixtures.Count(), leagueId);
            return fixtures;
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

    public async Task<IEnumerable<FixtureRawDto>?> GetHeadToHeadAsync(int homeTeamExternalId, int awayTeamExternalId, int last = 10)
    {
        if (!await rateLimiter.TryConsumeAsync())
        {
            return null;
        }

        try
        {
            logger.LogDebug("Fetching H2H for {Home} vs {Away}", homeTeamExternalId, awayTeamExternalId);

            var envelope = await httpClient.GetFromJsonAsync<FootballApiEnvelope<FixtureResponse>>(
                $"fixtures/headtohead?h2h={homeTeamExternalId}-{awayTeamExternalId}&last={last}", JsonOptions);

            if (envelope?.Response is null)
            {
                return [];
            }

            IEnumerable<FixtureRawDto> fixtures = envelope.Response.Select(MapToFixtureDto);
            logger.LogInformation("Fetched {Count} H2H matches for {Home} vs {Away}", fixtures.Count(), homeTeamExternalId, awayTeamExternalId);
            return fixtures;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch H2H for {Home} vs {Away}", homeTeamExternalId, awayTeamExternalId);
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

    private static FixtureRawDto MapToFixtureDto(FixtureResponse r) => new(
        ExternalId: r.Fixture.Id,
        KickoffUtc: r.Fixture.Date,
        StatusShort: r.Fixture.Status.Short,
        HomeScore: r.Goals.Home,
        AwayScore: r.Goals.Away,
        VenueName: r.Fixture.Venue?.Name,
        RefereeName: r.Fixture.Referee,

        HomeTeamExternalId: r.Teams.Home.Id,
        HomeTeamName: r.Teams.Home.Name,
        HomeTeamLogo: r.Teams.Home.Logo,

        AwayTeamExternalId: r.Teams.Away.Id,
        AwayTeamName: r.Teams.Away.Name,
        AwayTeamLogo: r.Teams.Away.Logo,

        LeagueExternalId: r.League.Id,
        LeagueName: r.League.Name,
        LeagueLogo: r.League.Logo,

        CountryName: r.League.Country,
        CountryCode: DeriveCountryCode(r.League.Country, r.League.Flag),
        CountryFlag: r.League.Flag,

        Season: r.League.Season.ToString(),
        Round: r.League.Round
    );

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

    /// <summary>
    /// Derive country code từ flag URL (e.g. ".../flags/gb.svg" → "GB")
    /// hoặc fallback về 3 ký tự đầu của country name.
    /// </summary>
    private static string DeriveCountryCode(string countryName, string? flagUrl)
    {
        if (!string.IsNullOrEmpty(flagUrl))
        {
            string fileName = Path.GetFileNameWithoutExtension(flagUrl.Split('/').Last());
            if (!string.IsNullOrEmpty(fileName))
            {
                return fileName.ToUpperInvariant()[..Math.Min(10, fileName.Length)];
            }
        }

        // Fallback: uppercase, no spaces, max 10 chars
        string normalized = countryName.ToUpperInvariant().Replace(" ", "");
        return normalized[..Math.Min(10, normalized.Length)];
    }

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
