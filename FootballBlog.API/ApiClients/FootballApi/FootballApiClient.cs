using System.Net;
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
    IApiKeyRotator keyRotator,
    IApiUsageTracker usageTracker,
    ILogger<FootballApiClient> logger) : IFootballApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<LiveMatch>?> GetAllLiveFixturesAsync()
    {
        string? key = await keyRotator.GetAvailableKeyAsync("FootballApi");
        if (key is null)
        {
            return null;
        }

        if (!await rateLimiter.TryConsumeAsync())
        {
            logger.LogWarning("GetAllLiveFixtures blocked by rate limit");
            return null;
        }

        try
        {
            logger.LogDebug("Fetching all live fixtures");

            var request = new HttpRequestMessage(HttpMethod.Get, "fixtures?live=all");
            request.Headers.Add("x-apisports-key", key);

            var response = await httpClient.SendAsync(request);

            if (await HandleRateLimitAsync(response, key, "fixtures?live=all"))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<FootballApiEnvelope<FixtureResponse>>(JsonOptions);

            if (envelope?.Response is null)
            {
                return [];
            }

            IEnumerable<LiveMatch> liveMatches = envelope.Response.Select(MapToLiveMatch);
            logger.LogInformation("Fetched {Count} live fixtures", liveMatches.Count());
            return liveMatches;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            logger.LogError(ex, "Failed to fetch live fixtures");
            return null;
        }
    }

    public async Task<IEnumerable<FixtureRawDto>?> GetHeadToHeadAsync(int homeTeamExternalId, int awayTeamExternalId, int last = 10)
    {
        string? key = await keyRotator.GetAvailableKeyAsync("FootballApi");
        if (key is null)
        {
            return null;
        }

        if (!await rateLimiter.TryConsumeAsync())
        {
            logger.LogWarning("GetHeadToHead blocked by rate limit — {HomeId} vs {AwayId}", homeTeamExternalId, awayTeamExternalId);
            return null;
        }

        if (!await usageTracker.CanCallAsync("FootballAPI"))
        {
            logger.LogWarning("GetHeadToHead blocked by daily quota — {HomeId} vs {AwayId}", homeTeamExternalId, awayTeamExternalId);
            return null;
        }

        try
        {
            logger.LogDebug("Fetching H2H for {Home} vs {Away}", homeTeamExternalId, awayTeamExternalId);

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"fixtures/headtohead?h2h={homeTeamExternalId}-{awayTeamExternalId}&last={last}");
            request.Headers.Add("x-apisports-key", key);

            var response = await httpClient.SendAsync(request);

            if (await HandleRateLimitAsync(response, key, $"fixtures/headtohead?h2h={homeTeamExternalId}-{awayTeamExternalId}"))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<FootballApiEnvelope<FixtureResponse>>(JsonOptions);

            if (envelope?.Response is null)
            {
                return [];
            }

            await usageTracker.IncrementAsync("FootballAPI");
            IEnumerable<FixtureRawDto> fixtures = envelope.Response.Select(MapToFixtureDto);
            logger.LogInformation("Fetched {Count} H2H matches for {Home} vs {Away}", fixtures.Count(), homeTeamExternalId, awayTeamExternalId);
            return fixtures;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            logger.LogError(ex, "Failed to fetch H2H for {Home} vs {Away}", homeTeamExternalId, awayTeamExternalId);
            return null;
        }
    }

    public async Task<string?> GetLineupsRawAsync(int fixtureId)
    {
        string? key = await keyRotator.GetAvailableKeyAsync("FootballApi");
        if (key is null)
        {
            return null;
        }

        if (!await rateLimiter.TryConsumeAsync())
        {
            logger.LogWarning("GetLineupsRaw blocked by rate limit — fixture {FixtureId}", fixtureId);
            return null;
        }

        try
        {
            logger.LogDebug("Fetching lineups for fixture {FixtureId}", fixtureId);

            var request = new HttpRequestMessage(HttpMethod.Get, $"fixtures/lineups?fixture={fixtureId}");
            request.Headers.Add("x-apisports-key", key);

            var response = await httpClient.SendAsync(request);

            if (await HandleRateLimitAsync(response, key, $"fixtures/lineups?fixture={fixtureId}"))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            logger.LogInformation("Fetched lineups for fixture {FixtureId}", fixtureId);
            return json;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            logger.LogError(ex, "Failed to fetch lineups for fixture {FixtureId}", fixtureId);
            return null;
        }
    }

    public async Task<IEnumerable<TeamRawDto>?> GetTeamsByLeagueAsync(int leagueId, int season)
    {
        string? key = await keyRotator.GetAvailableKeyAsync("FootballApi");
        if (key is null)
        {
            return null;
        }

        if (!await rateLimiter.TryConsumeAsync())
        {
            logger.LogWarning("GetTeamsByLeague blocked by rate limit — league {LeagueId}", leagueId);
            return null;
        }

        try
        {
            logger.LogDebug("Fetching teams for league {LeagueId} season {Season}", leagueId, season);

            var request = new HttpRequestMessage(HttpMethod.Get, $"teams?league={leagueId}&season={season}");
            request.Headers.Add("x-apisports-key", key);

            var response = await httpClient.SendAsync(request);

            if (await HandleRateLimitAsync(response, key, $"teams?league={leagueId}&season={season}"))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<FootballApiEnvelope<TeamResponse>>(JsonOptions);
            if (envelope?.Response is null)
            {
                return [];
            }

            if (HasApiErrors($"teams?league={leagueId}&season={season}", envelope.Errors))
            {
                return null;
            }

            IEnumerable<TeamRawDto> teams = envelope.Response.Select(r => new TeamRawDto(
                TeamExternalId: r.Team.Id,
                TeamName: r.Team.Name,
                TeamCode: r.Team.Code,
                TeamLogo: r.Team.Logo,
                CountryName: null,
                VenueExternalId: r.Venue?.Id,
                VenueName: r.Venue?.Name,
                VenueCity: r.Venue?.City,
                VenueCapacity: r.Venue?.Capacity,
                VenueImageUrl: r.Venue?.Image
            ));

            logger.LogInformation("Fetched {Count} teams for league {LeagueId}", teams.Count(), leagueId);
            return teams;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            logger.LogError(ex, "Failed to fetch teams for league {LeagueId}", leagueId);
            return null;
        }
    }

    public async Task<IEnumerable<StandingRawDto>?> GetStandingsAsync(int leagueId, int season)
    {
        string? key = await keyRotator.GetAvailableKeyAsync("FootballApi");
        if (key is null)
        {
            return null;
        }

        if (!await rateLimiter.TryConsumeAsync())
        {
            logger.LogWarning("GetStandings blocked by rate limit — league {LeagueId}", leagueId);
            return null;
        }

        try
        {
            logger.LogDebug("Fetching standings for league {LeagueId} season {Season}", leagueId, season);

            var request = new HttpRequestMessage(HttpMethod.Get, $"standings?league={leagueId}&season={season}");
            request.Headers.Add("x-apisports-key", key);

            var response = await httpClient.SendAsync(request);

            if (await HandleRateLimitAsync(response, key, $"standings?league={leagueId}&season={season}"))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<FootballApiEnvelope<StandingsEnvelope>>(JsonOptions);
            if (HasApiErrors($"standings?league={leagueId}&season={season}", envelope?.Errors ?? default))
            {
                return null;
            }

            if (envelope?.Response is null || envelope.Response.Length == 0)
            {
                return [];
            }

            StandingEntry[] entries = envelope.Response[0].League.Standings.Length > 0
                ? envelope.Response[0].League.Standings[0]
                : [];

            int apiSeason = envelope.Response[0].League.Season;

            IEnumerable<StandingRawDto> standings = entries.Select(e => new StandingRawDto(
                LeagueExternalId: leagueId,
                Season: apiSeason,
                TeamExternalId: e.Team.Id,
                TeamName: e.Team.Name,
                Rank: e.Rank,
                Points: e.Points,
                Played: e.All.Played,
                Won: e.All.Won,
                Drawn: e.All.Drawn,
                Lost: e.All.Lost,
                GoalsFor: e.All.Goals.For,
                GoalsAgainst: e.All.Goals.Against,
                GoalsDiff: e.GoalsDiff,
                Form: e.Form,
                Description: e.Description,
                Status: e.Status,
                UpdatedAt: e.UpdatedAt.UtcDateTime
            ));

            logger.LogInformation("Fetched {Count} standing entries for league {LeagueId}", standings.Count(), leagueId);
            return standings;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            logger.LogError(ex, "Failed to fetch standings for league {LeagueId}", leagueId);
            return null;
        }
    }

    public async Task<IEnumerable<FixtureRawDto>?> GetFixturesByRangeAsync(int leagueId, int season, DateOnly from, DateOnly to)
    {
        string? key = await keyRotator.GetAvailableKeyAsync("FootballApi");
        if (key is null)
        {
            return null;
        }

        if (!await rateLimiter.TryConsumeAsync())
        {
            logger.LogWarning("GetFixturesByRange blocked by rate limit — league {LeagueId}", leagueId);
            return null;
        }

        try
        {
            string fromStr = from.ToString("yyyy-MM-dd");
            string toStr = to.ToString("yyyy-MM-dd");
            logger.LogDebug("Fetching fixtures for league {LeagueId} season {Season} from {From} to {To}",
                leagueId, season, fromStr, toStr);

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"fixtures?league={leagueId}&season={season}&from={fromStr}&to={toStr}");
            request.Headers.Add("x-apisports-key", key);

            var response = await httpClient.SendAsync(request);

            if (await HandleRateLimitAsync(response, key, $"fixtures?league={leagueId}&season={season}&from={fromStr}&to={toStr}"))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<FootballApiEnvelope<FixtureResponse>>(JsonOptions);
            if (HasApiErrors($"fixtures?league={leagueId}&season={season}&from={fromStr}&to={toStr}", envelope?.Errors ?? default))
            {
                return null;
            }

            if (envelope?.Response is null)
            {
                return [];
            }

            IEnumerable<FixtureRawDto> fixtures = envelope.Response.Select(MapToFixtureDto);
            logger.LogInformation("Fetched {Count} fixtures for league {LeagueId} [{From} → {To}]",
                fixtures.Count(), leagueId, fromStr, toStr);
            return fixtures;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            logger.LogError(ex, "Failed to fetch fixtures for league {LeagueId}", leagueId);
            return null;
        }
    }

    public async Task<IEnumerable<FixtureRawDto>?> GetFixturesByDateAsync(DateOnly date)
    {
        string? key = await keyRotator.GetAvailableKeyAsync("FootballApi");
        if (key is null)
        {
            return null;
        }

        if (!await rateLimiter.TryConsumeAsync())
        {
            logger.LogWarning("GetFixturesByDate blocked by rate limit — date {Date}", date);
            return null;
        }

        if (!await usageTracker.CanCallAsync("FootballAPI"))
        {
            logger.LogWarning("GetFixturesByDate blocked by daily quota — date {Date}", date);
            return null;
        }

        try
        {
            string dateStr = date.ToString("yyyy-MM-dd");
            string endpoint = $"fixtures?date={dateStr}&timezone=Asia/Ho_Chi_Minh";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("x-apisports-key", key);

            var response = await httpClient.SendAsync(request);

            if (await HandleRateLimitAsync(response, key, endpoint))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<FootballApiEnvelope<FixtureResponse>>(JsonOptions);
            if (HasApiErrors(endpoint, envelope?.Errors ?? default))
            {
                return null;
            }

            if (envelope?.Response is null)
            {
                return [];
            }

            await usageTracker.IncrementAsync("FootballAPI");
            return envelope.Response.Select(MapToFixtureDto);
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            logger.LogError(ex, "Failed to fetch fixtures for date {Date}", date);
            return null;
        }
    }

    public async Task<HalfTimeContext?> GetFixtureHalfTimeDataAsync(int fixtureExternalId, int htHomeScore, int htAwayScore, int homeTeamExternalId, int awayTeamExternalId)
    {
        string? key = await keyRotator.GetAvailableKeyAsync("FootballApi");
        if (key is null)
        {
            return null;
        }

        // ── Statistics ────────────────────────────────────────────────────────
        if (!await rateLimiter.TryConsumeAsync()) { logger.LogWarning("GetFixtureHalfTimeData stats blocked by rate limit"); return null; }
        if (!await usageTracker.CanCallAsync("FootballAPI")) { logger.LogWarning("GetFixtureHalfTimeData stats blocked by daily quota"); return null; }

        List<FixtureStatisticsTeam>? statTeams = null;
        try
        {
            var statsEndpoint = $"fixtures/statistics?fixture={fixtureExternalId}";
            var req = new HttpRequestMessage(HttpMethod.Get, statsEndpoint);
            req.Headers.Add("x-apisports-key", key);
            var res = await httpClient.SendAsync(req);
            if (!await HandleRateLimitAsync(res, key, statsEndpoint))
            {
                res.EnsureSuccessStatusCode();
                var envelope = await res.Content.ReadFromJsonAsync<FootballApiEnvelope<FixtureStatisticsTeam>>(JsonOptions);
                statTeams = envelope?.Response?.ToList();
                await usageTracker.IncrementAsync("FootballAPI");
            }
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            logger.LogError(ex, "Failed to fetch fixture statistics for {FixtureId}", fixtureExternalId);
        }

        // ── Events ────────────────────────────────────────────────────────────
        key = await keyRotator.GetAvailableKeyAsync("FootballApi");
        if (key is null)
        {
            return null;
        }

        if (!await rateLimiter.TryConsumeAsync()) { logger.LogWarning("GetFixtureHalfTimeData events blocked by rate limit"); return null; }
        if (!await usageTracker.CanCallAsync("FootballAPI")) { logger.LogWarning("GetFixtureHalfTimeData events blocked by daily quota"); return null; }

        List<FixtureEvent>? h1Events = null;
        try
        {
            var eventsEndpoint = $"fixtures/events?fixture={fixtureExternalId}";
            var req = new HttpRequestMessage(HttpMethod.Get, eventsEndpoint);
            req.Headers.Add("x-apisports-key", key);
            var res = await httpClient.SendAsync(req);
            if (!await HandleRateLimitAsync(res, key, eventsEndpoint))
            {
                res.EnsureSuccessStatusCode();
                var envelope = await res.Content.ReadFromJsonAsync<FootballApiEnvelope<FixtureEvent>>(JsonOptions);
                // Chỉ lấy events trong H1 (elapsed <= 45)
                h1Events = envelope?.Response?.Where(e => e.Time.Elapsed <= 45).ToList();
                await usageTracker.IncrementAsync("FootballAPI");
            }
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            logger.LogError(ex, "Failed to fetch fixture events for {FixtureId}", fixtureExternalId);
        }

        // ── Build HalfTimeContext ─────────────────────────────────────────────
        string? GetStat(List<FixtureStatisticsTeam>? teams, int teamExternalId, string statType)
        {
            var team = teams?.FirstOrDefault(t => t.Team.Id == teamExternalId);
            if (team is null)
            {
                return null;
            }

            var item = team.Statistics.FirstOrDefault(s => s.Type == statType);
            if (item is null)
            {
                return null;
            }

            return item.Value.ValueKind == System.Text.Json.JsonValueKind.Null ? null : item.Value.ToString();
        }

        int? ParseStatInt(string? raw) => int.TryParse(raw?.TrimEnd('%'), out var v) ? v : null;

        var halfTimeEvents = h1Events?.Select(e => new HalfTimeEvent(
            e.Time.Elapsed,
            e.Type,
            e.Team.Name,
            e.Player?.Name
        )).ToList() ?? [];

        logger.LogInformation("Fetched HT data for fixture {FixtureId}: stats={HasStats}, events={EventCount}",
            fixtureExternalId, statTeams is not null, halfTimeEvents.Count);

        return new HalfTimeContext(
            HtHomeScore: htHomeScore,
            HtAwayScore: htAwayScore,
            HomePossession: GetStat(statTeams, homeTeamExternalId, "Ball Possession"),
            AwayPossession: GetStat(statTeams, awayTeamExternalId, "Ball Possession"),
            HomeShotsOnTarget: ParseStatInt(GetStat(statTeams, homeTeamExternalId, "Shots on Goal")),
            AwayShotsOnTarget: ParseStatInt(GetStat(statTeams, awayTeamExternalId, "Shots on Goal")),
            HomeCorners: ParseStatInt(GetStat(statTeams, homeTeamExternalId, "Corner Kicks")),
            AwayCorners: ParseStatInt(GetStat(statTeams, awayTeamExternalId, "Corner Kicks")),
            HomeYellowCards: ParseStatInt(GetStat(statTeams, homeTeamExternalId, "Yellow Cards")),
            AwayYellowCards: ParseStatInt(GetStat(statTeams, awayTeamExternalId, "Yellow Cards")),
            H1Events: halfTimeEvents
        );
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Đọc response body khi 429/403 để phân biệt per-minute vs daily limit,
    /// gọi MarkExhaustedAsync với TTL phù hợp (65s vs midnight UTC).
    /// Returns true nếu là rate limit response (caller nên return null).
    /// </summary>
    private async Task<bool> HandleRateLimitAsync(HttpResponseMessage response, string apiKey, string endpoint)
    {
        if (response.StatusCode is not (HttpStatusCode.TooManyRequests or HttpStatusCode.Forbidden))
        {
            return false;
        }

        string body = await response.Content.ReadAsStringAsync();

        // API-Football: daily limit chứa "day"/"exceeded"/"requests"
        // Per-minute limit chứa "minute"/"rate"
        bool isDailyLimit = body.Contains("day", StringComparison.OrdinalIgnoreCase)
                         || body.Contains("exceeded", StringComparison.OrdinalIgnoreCase);

        logger.LogWarning(
            "Football API {Status} on [{Endpoint}] — {LimitType} | body: {Body}",
            (int)response.StatusCode, endpoint,
            isDailyLimit ? "DAILY LIMIT" : "per-minute rate limit",
            body);

        await keyRotator.MarkExhaustedAsync("FootballApi", apiKey, isDailyLimit);
        return true;
    }

    private bool HasApiErrors(string endpoint, JsonElement errors)
    {
        if (errors.ValueKind == JsonValueKind.Object)
        {
            logger.LogWarning("Football API returned errors on {Endpoint}: {Errors}", endpoint, errors);
            return true;
        }
        return false;
    }

    // ── Mappers ────────────────────────────────────────────────────────────────

    private static FixtureRawDto MapToFixtureDto(FixtureResponse r) => new(
        ExternalId: r.Fixture.Id,
        KickoffUtc: r.Fixture.Date.UtcDateTime,
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
        StartedAt = r.Fixture.Date.UtcDateTime
    };

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

        string normalized = countryName.ToUpperInvariant().Replace(" ", "");
        return normalized[..Math.Min(10, normalized.Length)];
    }

    private static MatchStatus MapStatus(string s) => s switch
    {
        "NS" => MatchStatus.Scheduled,
        "1H" or "2H" or "ET" or "P" or "LIVE" or "BT" => MatchStatus.Live,
        "HT" => MatchStatus.HalfTime,
        "FT" or "AET" or "PEN" => MatchStatus.Finished,
        "PST" => MatchStatus.Postponed,
        "SUSP" or "CANC" or "ABD" or "WO" => MatchStatus.Cancelled,
        _ => MatchStatus.Scheduled
    };
}
