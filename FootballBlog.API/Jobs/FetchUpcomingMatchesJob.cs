using System.Diagnostics;
using FootballBlog.Core.DTOs;
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
        var sw = Stopwatch.StartNew();
        FootballApiOptions opts = options.Value;

        // 1. Lấy các ngày đã có data trong DB (từ hôm nay trở đi)
        HashSet<DateOnly> fetchedDates = await uow.Matches.GetFetchedDatesAsync();

        // 2. Tính window [today, tomorrow] và lọc ngày chưa có data
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        List<DateOnly> datesToFetch = Enumerable.Range(0, 2)
            .Select(i => today.AddDays(i))
            .Where(d => !fetchedDates.Contains(d))
            .ToList();

        logger.LogInformation(
            "FetchUpcomingMatchesJob started. Leagues={LeagueCount}, DatesToFetch={Count} [{Dates}]",
            opts.LeagueIds.Length, datesToFetch.Count, string.Join(", ", datesToFetch));

        if (datesToFetch.Count == 0)
        {
            logger.LogInformation("Tất cả ngày trong window đã có data, skip. Duration={DurationMs}ms", sw.ElapsedMilliseconds);
            return;
        }

        int newMatches = 0;
        int updatedMatches = 0;
        DateTime now = DateTime.UtcNow;

        // Cache trong 1 run để tránh lookup DB lặp lại cho cùng country/league/team
        var countryCache = new Dictionary<string, int>();
        var leagueCache = new Dictionary<int, int>();
        var teamCache = new Dictionary<int, int>();

        // 3. Fetch từng ngày × từng league — commit sau mỗi ngày hoàn thành
        foreach (DateOnly date in datesToFetch)
        {
            bool dateAborted = false;
            int dateNew = 0;
            int dateUpdated = 0;

            foreach (int configLeagueId in opts.LeagueIds)
            {
                IEnumerable<FixtureRawDto>? fixtures = await apiClient.GetFixturesByDateAsync(configLeagueId, date);
                if (fixtures is null)
                {
                    logger.LogWarning(
                        "FetchUpcomingMatchesJob aborted at league {LeagueId} date {Date} — null response (rate limit or HTTP error)",
                        configLeagueId, date);
                    dateAborted = true;
                    break;
                }

                foreach (FixtureRawDto fixture in fixtures)
                {
                    // Thứ tự upsert bắt buộc do FK dependency
                    int countryId = await UpsertCountryAsync(fixture, countryCache);
                    int leagueId = await UpsertLeagueAsync(fixture, countryId, leagueCache);
                    int homeTeamId = await UpsertTeamAsync(fixture.HomeTeamExternalId, fixture.HomeTeamName, fixture.HomeTeamLogo, countryId, teamCache);
                    int awayTeamId = await UpsertTeamAsync(fixture.AwayTeamExternalId, fixture.AwayTeamName, fixture.AwayTeamLogo, countryId, teamCache);

                    Match? existing = await uow.Matches.GetByExternalIdAsync(fixture.ExternalId);

                    if (existing is null)
                    {
                        Match newMatch = new()
                        {
                            ExternalId = fixture.ExternalId,
                            HomeTeamId = homeTeamId,
                            AwayTeamId = awayTeamId,
                            LeagueId = leagueId,
                            Season = fixture.Season,
                            Round = fixture.Round,
                            KickoffUtc = fixture.KickoffUtc,
                            Status = MapStatus(fixture.StatusShort),
                            HomeScore = fixture.HomeScore,
                            AwayScore = fixture.AwayScore,
                            VenueName = fixture.VenueName,
                            RefereeName = fixture.RefereeName,
                            FetchedAt = now
                        };
                        await uow.Matches.AddAsync(newMatch);
                        newMatches++;
                        dateNew++;

                        // Schedule pre-match data jobs — chỉ khi thời gian còn trong tương lai
                        DateTime h2hTime = fixture.KickoffUtc.AddHours(-5);
                        DateTime lineupTime = fixture.KickoffUtc.AddMinutes(-15);

                        if (h2hTime > now)
                        {
                            BackgroundJob.Schedule<PreMatchDataJob>(
                                j => j.FetchH2HAsync(fixture.ExternalId, fixture.HomeTeamExternalId, fixture.AwayTeamExternalId),
                                h2hTime);

                            logger.LogDebug("Scheduled H2H fetch for fixture {FixtureId} at {Time}",
                                fixture.ExternalId, h2hTime);
                        }

                        if (lineupTime > now)
                        {
                            BackgroundJob.Schedule<PreMatchDataJob>(
                                j => j.FetchLineupsAsync(fixture.ExternalId),
                                lineupTime);

                            logger.LogDebug("Scheduled lineup fetch for fixture {FixtureId} at {Time}",
                                fixture.ExternalId, lineupTime);
                        }
                    }
                    else
                    {
                        // Idempotent update — chỉ cập nhật status + score + fetchedAt
                        existing.Status = MapStatus(fixture.StatusShort);
                        existing.HomeScore = fixture.HomeScore;
                        existing.AwayScore = fixture.AwayScore;
                        existing.FetchedAt = now;
                        await uow.Matches.UpdateAsync(existing);
                        updatedMatches++;
                        dateUpdated++;
                    }
                }
            }

            // Commit sau mỗi ngày hoàn thành — nếu abort thì không commit, ngày đó sẽ retry lần sau
            if (!dateAborted)
            {
                await uow.CommitAsync();
                logger.LogInformation("Committed fixtures for {Date}. New={New}, Updated={Updated}",
                    date, dateNew, dateUpdated);
            }
            else
            {
                // Rate limit hoặc HTTP error — dừng toàn bộ, không thử các ngày còn lại
                break;
            }
        }

        sw.Stop();
        logger.LogInformation(
            "FetchUpcomingMatchesJob finished. New={NewMatches}, Updated={UpdatedMatches}, Duration={DurationMs}ms",
            newMatches, updatedMatches, sw.ElapsedMilliseconds);
    }

    // ── Upsert helpers ────────────────────────────────────────────────────────

    private async Task<int> UpsertCountryAsync(FixtureRawDto fixture, Dictionary<string, int> cache)
    {
        if (cache.TryGetValue(fixture.CountryCode, out int cached))
        {
            return cached;
        }

        Country? existing = await uow.Countries.GetByCodeAsync(fixture.CountryCode);
        if (existing is not null)
        {
            cache[fixture.CountryCode] = existing.Id;
            return existing.Id;
        }

        Country country = new()
        {
            Code = fixture.CountryCode,
            Name = fixture.CountryName,
            FlagUrl = fixture.CountryFlag
        };
        await uow.Countries.AddAsync(country);
        await uow.CommitAsync(); // commit ngay để lấy real ID

        cache[fixture.CountryCode] = country.Id;
        logger.LogInformation("Upserted new country: {Name} ({Code})", country.Name, country.Code);
        return country.Id;
    }

    private async Task<int> UpsertLeagueAsync(FixtureRawDto fixture, int countryId, Dictionary<int, int> cache)
    {
        if (cache.TryGetValue(fixture.LeagueExternalId, out int cached))
        {
            return cached;
        }

        League? existing = await uow.Leagues.GetByExternalIdAsync(fixture.LeagueExternalId);
        if (existing is not null)
        {
            // Cập nhật tên nếu API đổi
            if (existing.Name != fixture.LeagueName)
            {
                existing.Name = fixture.LeagueName;
                await uow.Leagues.UpdateAsync(existing);
                await uow.CommitAsync();
            }

            cache[fixture.LeagueExternalId] = existing.Id;
            return existing.Id;
        }

        League league = new()
        {
            ExternalId = fixture.LeagueExternalId,
            Name = fixture.LeagueName,
            LogoUrl = fixture.LeagueLogo,
            CountryId = countryId,
            IsActive = true
        };
        await uow.Leagues.AddAsync(league);
        await uow.CommitAsync();

        cache[fixture.LeagueExternalId] = league.Id;
        logger.LogInformation("Upserted new league: {Name} (ext={ExternalId})", league.Name, league.ExternalId);
        return league.Id;
    }

    private async Task<int> UpsertTeamAsync(int externalId, string name, string? logo, int countryId, Dictionary<int, int> cache)
    {
        if (cache.TryGetValue(externalId, out int cached))
        {
            return cached;
        }

        Team? existing = await uow.Teams.GetByExternalIdAsync(externalId);
        if (existing is not null)
        {
            // Cập nhật tên nếu API đổi (ví dụ: đội đổi tên chính thức)
            if (existing.Name != name)
            {
                existing.Name = name;
                await uow.Teams.UpdateAsync(existing);
                await uow.CommitAsync();
            }

            cache[externalId] = existing.Id;
            return existing.Id;
        }

        Team team = new()
        {
            ExternalId = externalId,
            Name = name,
            LogoUrl = logo,
            CountryId = countryId
        };
        await uow.Teams.AddAsync(team);
        await uow.CommitAsync();

        cache[externalId] = team.Id;
        logger.LogInformation("Upserted new team: {Name} (ext={ExternalId})", team.Name, team.ExternalId);
        return team.Id;
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
