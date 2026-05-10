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
    IFootballApiRateLimiter rateLimiter,
    IUnitOfWork uow,
    IOptions<FootballApiOptions> options,
    ILogger<FetchUpcomingMatchesJob> logger)
{
    public async Task ExecuteAsync()
    {
        var sw = Stopwatch.StartNew();
        FootballApiOptions opts = options.Value;

        // Tính ngày theo giờ VN (UTC+7) để tránh lệch ngày khi job chạy đêm UTC
        var vnZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        DateOnly today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone));
        // Luôn fetch cả 3 ngày: hôm qua cập nhật kết quả FT, hôm nay cập nhật live, ngày mai sync postpone/thêm mới
        List<DateOnly> datesToFetch = [today.AddDays(-1), today, today.AddDays(1)];

        int usageToday = await rateLimiter.GetTodayUsageAsync();
        logger.LogInformation(
            "FetchUpcomingMatchesJob started. Leagues={LeagueCount}, Dates=[{Dates}], ApiUsageToday={Usage}/100",
            opts.LeagueIds.Length, string.Join(", ", datesToFetch), usageToday);

        int newMatches = 0;
        int updatedMatches = 0;
        DateTime now = DateTime.UtcNow;

        // Cache trong 1 run để tránh lookup DB lặp lại cho cùng country/league/team
        var countryCache = new Dictionary<string, Country>();
        var leagueCache = new Dictionary<int, League>();
        var teamCache = new Dictionary<int, Team>();

        var allowedLeagues = new HashSet<int>(opts.LeagueIds);

        // 3. Fetch 1 request/ngày — trả về tất cả giải, lọc theo LeagueIds trong config
        foreach (DateOnly date in datesToFetch)
        {
            int dateNew = 0;
            int dateUpdated = 0;

            IEnumerable<FixtureRawDto>? allFixtures = await apiClient.GetFixturesByDateAsync(date);
            if (allFixtures is null)
            {
                int usage = await rateLimiter.GetTodayUsageAsync();
                logger.LogError(
                    "FetchUpcomingMatchesJob aborted at {Date} — API returned null. Usage={Usage}/100. " +
                    "Likely cause: rate limit hit (see warnings above) or no API key configured. Remaining dates skipped.",
                    date, usage);
                break;
            }

            List<FixtureRawDto> fixtures = allFixtures.Where(f => allowedLeagues.Contains(f.LeagueExternalId)).ToList();
            logger.LogInformation("Date {Date}: {Total} total fixtures from API, {Filtered} match config leagues",
                date, allFixtures.Count(), fixtures.Count);

            foreach (FixtureRawDto fixture in fixtures)
            {
                // Thứ tự upsert bắt buộc do FK dependency — dùng navigation property để tránh commit per-entity
                Country country = await UpsertCountryAsync(fixture, countryCache);
                League league = await UpsertLeagueAsync(fixture, country, leagueCache);
                Team homeTeam = await UpsertTeamAsync(fixture.HomeTeamExternalId, fixture.HomeTeamName, fixture.HomeTeamLogo, country, teamCache);
                Team awayTeam = await UpsertTeamAsync(fixture.AwayTeamExternalId, fixture.AwayTeamName, fixture.AwayTeamLogo, country, teamCache);

                Match? existing = await uow.Matches.GetByExternalIdAsync(fixture.ExternalId);

                if (existing is null)
                {
                    Match newMatch = new()
                    {
                        ExternalId = fixture.ExternalId,
                        HomeTeam = homeTeam,
                        AwayTeam = awayTeam,
                        League = league,
                        Season = fixture.Season,
                        Round = fixture.Round,
                        KickoffUtc = fixture.KickoffUtc,
                        Status = MapStatus(fixture.StatusShort),
                        HomeScore = fixture.HomeScore,
                        AwayScore = fixture.AwayScore,
                        HtHomeScore = fixture.HtHomeScore,
                        HtAwayScore = fixture.HtAwayScore,
                        EtHomeScore = fixture.EtHomeScore,
                        EtAwayScore = fixture.EtAwayScore,
                        PenHomeScore = fixture.PenHomeScore,
                        PenAwayScore = fixture.PenAwayScore,
                        VenueName = fixture.VenueName,
                        RefereeName = fixture.RefereeName,
                        FetchedAt = now
                    };
                    await uow.Matches.AddAsync(newMatch);
                    newMatches++;
                    dateNew++;
                }
                else
                {
                    // Idempotent update — cập nhật mọi field có thể thay đổi sau lần fetch đầu
                    existing.Status = MapStatus(fixture.StatusShort);
                    existing.HomeScore = fixture.HomeScore;
                    existing.AwayScore = fixture.AwayScore;
                    // HT/ET/Pen score chỉ ghi đè khi API trả về — tránh xóa data đã có
                    if (fixture.HtHomeScore.HasValue)
                    {
                        existing.HtHomeScore = fixture.HtHomeScore;
                    }

                    if (fixture.HtAwayScore.HasValue)
                    {
                        existing.HtAwayScore = fixture.HtAwayScore;
                    }

                    if (fixture.EtHomeScore.HasValue)
                    {
                        existing.EtHomeScore = fixture.EtHomeScore;
                    }

                    if (fixture.EtAwayScore.HasValue)
                    {
                        existing.EtAwayScore = fixture.EtAwayScore;
                    }

                    if (fixture.PenHomeScore.HasValue)
                    {
                        existing.PenHomeScore = fixture.PenHomeScore;
                    }

                    if (fixture.PenAwayScore.HasValue)
                    {
                        existing.PenAwayScore = fixture.PenAwayScore;
                    }
                    // Referee thường null lúc fetch sớm, API bổ sung sau khi trận bắt đầu
                    if (!string.IsNullOrEmpty(fixture.RefereeName))
                    {
                        existing.RefereeName = fixture.RefereeName;
                    }

                    if (!string.IsNullOrEmpty(fixture.Round))
                    {
                        existing.Round = fixture.Round;
                    }

                    if (!string.IsNullOrEmpty(fixture.VenueName))
                    {
                        existing.VenueName = fixture.VenueName;
                    }

                    existing.FetchedAt = now;
                    await uow.Matches.UpdateAsync(existing);
                    updatedMatches++;
                    dateUpdated++;

                }
            }

            await uow.CommitAsync();
            logger.LogInformation("Committed fixtures for {Date}. New={New}, Updated={Updated}",
                date, dateNew, dateUpdated);
        }

        sw.Stop();
        int finalUsage = await rateLimiter.GetTodayUsageAsync();
        logger.LogInformation(
            "FetchUpcomingMatchesJob finished. New={NewMatches}, Updated={UpdatedMatches}, Duration={DurationMs}ms, ApiUsageToday={Usage}/100",
            newMatches, updatedMatches, sw.ElapsedMilliseconds, finalUsage);
    }

    // ── Upsert helpers ────────────────────────────────────────────────────────
    // Trả về entity object và dùng navigation property để EF Core tự resolve FK —
    // không CommitAsync trong helper, chỉ commit 1 lần/ngày ở caller.

    private async Task<Country> UpsertCountryAsync(FixtureRawDto fixture, Dictionary<string, Country> cache)
    {
        if (cache.TryGetValue(fixture.CountryCode, out Country? cached))
        {
            return cached;
        }

        Country? existing = await uow.Countries.GetByCodeAsync(fixture.CountryCode);
        if (existing is not null)
        {
            cache[fixture.CountryCode] = existing;
            return existing;
        }

        Country country = new()
        {
            Code = fixture.CountryCode,
            Name = fixture.CountryName,
            FlagUrl = fixture.CountryFlag
        };
        await uow.Countries.AddAsync(country);

        cache[fixture.CountryCode] = country;
        logger.LogInformation("Upserted new country: {Name} ({Code})", country.Name, country.Code);
        return country;
    }

    private async Task<League> UpsertLeagueAsync(FixtureRawDto fixture, Country country, Dictionary<int, League> cache)
    {
        if (cache.TryGetValue(fixture.LeagueExternalId, out League? cached))
        {
            return cached;
        }

        League? existing = await uow.Leagues.GetByExternalIdAsync(fixture.LeagueExternalId);
        if (existing is not null)
        {
            // Cập nhật tên nếu API đổi — commit gộp vào cuối ngày
            if (existing.Name != fixture.LeagueName)
            {
                existing.Name = fixture.LeagueName;
                await uow.Leagues.UpdateAsync(existing);
            }

            cache[fixture.LeagueExternalId] = existing;
            return existing;
        }

        League league = new()
        {
            ExternalId = fixture.LeagueExternalId,
            Name = fixture.LeagueName,
            LogoUrl = fixture.LeagueLogo,
            Country = country, // navigation property — EF tự resolve CountryId
            IsActive = true
        };
        await uow.Leagues.AddAsync(league);

        cache[fixture.LeagueExternalId] = league;
        logger.LogInformation("Upserted new league: {Name} (ext={ExternalId})", league.Name, league.ExternalId);
        return league;
    }

    private async Task<Team> UpsertTeamAsync(int externalId, string name, string? logo, Country country, Dictionary<int, Team> cache)
    {
        if (cache.TryGetValue(externalId, out Team? cached))
        {
            return cached;
        }

        Team? existing = await uow.Teams.GetByExternalIdAsync(externalId);
        if (existing is not null)
        {
            // Cập nhật tên nếu API đổi — commit gộp vào cuối ngày
            if (existing.Name != name)
            {
                existing.Name = name;
                await uow.Teams.UpdateAsync(existing);
            }

            cache[externalId] = existing;
            return existing;
        }

        Team team = new()
        {
            ExternalId = externalId,
            Name = name,
            LogoUrl = logo,
            Country = country // navigation property — EF tự resolve CountryId
        };
        await uow.Teams.AddAsync(team);

        cache[externalId] = team;
        logger.LogInformation("Upserted new team: {Name} (ext={ExternalId})", team.Name, team.ExternalId);
        return team;
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
