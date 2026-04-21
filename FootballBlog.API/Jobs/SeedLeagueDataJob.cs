using System.Diagnostics;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FootballBlog.API.Jobs;

/// <summary>
/// Job chạy 1 lần để seed toàn bộ dữ liệu các giải đấu vào DB phục vụ test UI.
/// Trigger thủ công từ Admin UI — KHÔNG schedule tự động.
/// DB-first pattern: check DB trước, chỉ gọi API khi chưa có data.
/// </summary>
public class SeedLeagueDataJob(
    IFootballApiClient apiClient,
    IUnitOfWork uow,
    IOptions<FootballApiOptions> options,
    ILogger<SeedLeagueDataJob> logger)
{
    public async Task ExecuteAsync()
    {
        var sw = Stopwatch.StartNew();
        FootballApiOptions opts = options.Value;
        int season = CurrentSeason(opts);

        logger.LogInformation("SeedLeagueDataJob started. Leagues={LeagueCount}, Season={Season}",
            opts.LeagueIds.Length, season);

        int teamsUpserted = 0;
        int venuesUpserted = 0;
        int standingsUpserted = 0;
        int fixturesUpserted = 0;

        // Cache trong 1 run để tránh lookup DB lặp lại
        var countryCache = new Dictionary<string, int>();
        var leagueCache = new Dictionary<int, int>();
        var teamCache = new Dictionary<int, int>();
        var venueCache = new Dictionary<int, int>();

        foreach (int leagueId in opts.LeagueIds)
        {
            logger.LogInformation("Processing league {LeagueId}...", leagueId);

            // Pre-check DB — một lần, dùng chung cho Step 1 và Step 2
            // Nếu fixtures đã có → teams đã được seed trước đó (fixtures require teams)
            int? internalLeagueId = await GetInternalLeagueIdAsync(leagueId, leagueCache);
            bool hasFixtures = internalLeagueId.HasValue &&
                               await uow.Matches.HasFixturesForLeagueAsync(internalLeagueId.Value, season.ToString());

            // ── Step 1: Teams + Venues ──────────────────────────────────────
            // Chỉ fetch khi fixtures chưa có — fixtures tồn tại đồng nghĩa teams đã được seed.
            if (hasFixtures)
            {
                logger.LogInformation("League {LeagueId} already seeded — skipping teams API call", leagueId);
            }
            else
            {
                IEnumerable<TeamRawDto>? teams = await apiClient.GetTeamsByLeagueAsync(leagueId, season);
                if (teams is null)
                {
                    logger.LogWarning("Skipping league {LeagueId} — null response from GetTeamsByLeague", leagueId);
                    continue;
                }

                foreach (TeamRawDto teamDto in teams)
                {
                    int? venueId = null;
                    if (teamDto.VenueExternalId.HasValue)
                    {
                        venueId = await UpsertVenueAsync(teamDto, venueCache);
                        venuesUpserted++;
                    }

                    await UpsertTeamWithVenueAsync(teamDto, venueId, teamCache);
                    teamsUpserted++;
                }

                await uow.CommitAsync();
            }

            // ── Step 2: Fixtures ──────────────────────────────────────────
            // Phải chạy TRƯỚC standings vì step này upsert Country + League vào DB.
            // Standings cần internalLeagueId → League phải tồn tại trước.
            if (hasFixtures)
            {
                logger.LogInformation("Fixtures for league {LeagueId} season {Season} already in DB — skipping", leagueId, season);
            }
            else
            {
                DateOnly seasonStart = new DateOnly(season, 7, 1);
                DateOnly seasonEnd = new DateOnly(season + 1, 7, 1);
                IEnumerable<FixtureRawDto>? fixtures = await apiClient.GetFixturesByRangeAsync(
                    leagueId, season,
                    from: seasonStart,
                    to: seasonEnd);

                if (fixtures is null)
                {
                    logger.LogWarning("Skipping league {LeagueId} — null response from GetFixturesByRange", leagueId);
                    continue;
                }

                foreach (FixtureRawDto fixture in fixtures)
                {
                    int countryId = await UpsertCountryAsync(fixture, countryCache);
                    int leagueInternalId = await UpsertLeagueAsync(fixture, countryId, leagueCache);
                    int homeTeamId = await UpsertTeamFromFixtureAsync(fixture.HomeTeamExternalId, fixture.HomeTeamName, fixture.HomeTeamLogo, countryId, teamCache);
                    int awayTeamId = await UpsertTeamFromFixtureAsync(fixture.AwayTeamExternalId, fixture.AwayTeamName, fixture.AwayTeamLogo, countryId, teamCache);

                    await UpsertMatchAsync(fixture, homeTeamId, awayTeamId, leagueInternalId);
                    fixturesUpserted++;
                }

                await uow.CommitAsync();

                // Cập nhật internalLeagueId sau khi fixtures đã upsert League vào DB
                internalLeagueId = await GetInternalLeagueIdAsync(leagueId, leagueCache);
            }

            // ── Step 3: Standings ───────────────────────────────────────────
            // League đã tồn tại sau Step 2 → internalLeagueId luôn có giá trị ở đây.
            if (internalLeagueId is null)
            {
                logger.LogWarning("League {LeagueId} not in DB after fixture fetch — skipping standings", leagueId);
                continue;
            }

            bool hasStandings = await uow.Standings.HasDataForSeasonAsync(internalLeagueId.Value, season);
            if (hasStandings)
            {
                logger.LogInformation("Standings for league {LeagueId} season {Season} already in DB — skipping", leagueId, season);
            }
            else
            {
                IEnumerable<StandingRawDto>? standings = await apiClient.GetStandingsAsync(leagueId, season);
                if (standings is null)
                {
                    logger.LogWarning("Skipping league {LeagueId} standings — null response", leagueId);
                }
                else
                {
                    foreach (StandingRawDto s in standings)
                    {
                        await UpsertStandingAsync(s, internalLeagueId.Value, teamCache);
                        standingsUpserted++;
                    }

                    await uow.CommitAsync();
                }
            }
        }

        sw.Stop();
        logger.LogInformation(
            "SeedLeagueDataJob finished. Teams={Teams}, Venues={Venues}, Standings={Standings}, Fixtures={Fixtures}, Duration={DurationMs}ms",
            teamsUpserted, venuesUpserted, standingsUpserted, fixturesUpserted, sw.ElapsedMilliseconds);
    }

    // ── Upsert helpers ────────────────────────────────────────────────────────

    private async Task<int> UpsertVenueAsync(TeamRawDto dto, Dictionary<int, int> cache)
    {
        int externalId = dto.VenueExternalId!.Value;
        if (cache.TryGetValue(externalId, out int cached))
        {
            return cached;
        }

        Venue? existing = await uow.Venues.GetByExternalIdAsync(externalId);
        if (existing is not null)
        {
            cache[externalId] = existing.Id;
            return existing.Id;
        }

        Venue venue = new()
        {
            ExternalId = externalId,
            Name = dto.VenueName ?? string.Empty,
            City = dto.VenueCity,
            Capacity = dto.VenueCapacity,
            ImageUrl = dto.VenueImageUrl
        };
        await uow.Venues.AddAsync(venue);
        await uow.CommitAsync();

        cache[externalId] = venue.Id;
        return venue.Id;
    }

    private async Task UpsertTeamWithVenueAsync(TeamRawDto dto, int? venueId, Dictionary<int, int> cache)
    {
        Team? existing = await uow.Teams.GetByExternalIdAsync(dto.TeamExternalId);
        if (existing is not null)
        {
            // Cập nhật VenueId nếu chưa có
            if (existing.VenueId is null && venueId.HasValue)
            {
                existing.VenueId = venueId;
                await uow.Teams.UpdateAsync(existing);
            }

            cache[dto.TeamExternalId] = existing.Id;
            return;
        }

        Team team = new()
        {
            ExternalId = dto.TeamExternalId,
            Name = dto.TeamName,
            ShortName = dto.TeamCode,
            LogoUrl = dto.TeamLogo,
            VenueId = venueId
        };
        await uow.Teams.AddAsync(team);
        await uow.CommitAsync();

        cache[dto.TeamExternalId] = team.Id;
        logger.LogDebug("Upserted team {Name} (ext={ExternalId})", team.Name, team.ExternalId);
    }

    private async Task UpsertStandingAsync(StandingRawDto dto, int leagueId, Dictionary<int, int> teamCache)
    {
        if (!teamCache.TryGetValue(dto.TeamExternalId, out int teamId))
        {
            Team? team = await uow.Teams.GetByExternalIdAsync(dto.TeamExternalId);
            if (team is null)
            {
                logger.LogWarning("Team ext={ExternalId} not found in DB for standing upsert", dto.TeamExternalId);
                return;
            }

            teamId = team.Id;
            teamCache[dto.TeamExternalId] = teamId;
        }

        Standing? existing = await uow.Standings.GetByLeagueTeamSeasonAsync(leagueId, teamId, dto.Season);
        if (existing is not null)
        {
            existing.Rank = dto.Rank;
            existing.Points = dto.Points;
            existing.Played = dto.Played;
            existing.Won = dto.Won;
            existing.Drawn = dto.Drawn;
            existing.Lost = dto.Lost;
            existing.GoalsFor = dto.GoalsFor;
            existing.GoalsAgainst = dto.GoalsAgainst;
            existing.GoalsDiff = dto.GoalsDiff;
            existing.Form = dto.Form;
            existing.Description = dto.Description;
            existing.Status = dto.Status;
            existing.UpdatedAt = dto.UpdatedAt;
            await uow.Standings.UpdateAsync(existing);
            return;
        }

        Standing standing = new()
        {
            LeagueId = leagueId,
            TeamId = teamId,
            Season = dto.Season,
            Rank = dto.Rank,
            Points = dto.Points,
            Played = dto.Played,
            Won = dto.Won,
            Drawn = dto.Drawn,
            Lost = dto.Lost,
            GoalsFor = dto.GoalsFor,
            GoalsAgainst = dto.GoalsAgainst,
            GoalsDiff = dto.GoalsDiff,
            Form = dto.Form,
            Description = dto.Description,
            Status = dto.Status,
            UpdatedAt = dto.UpdatedAt
        };
        await uow.Standings.AddAsync(standing);
    }

    private async Task UpsertMatchAsync(FixtureRawDto fixture, int homeTeamId, int awayTeamId, int leagueId)
    {
        Match? existing = await uow.Matches.GetByExternalIdAsync(fixture.ExternalId);
        if (existing is not null)
        {
            existing.Status = MapStatus(fixture.StatusShort);
            existing.HomeScore = fixture.HomeScore;
            existing.AwayScore = fixture.AwayScore;
            await uow.Matches.UpdateAsync(existing);
            return;
        }

        Match match = new()
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
            FetchedAt = DateTime.UtcNow
        };
        await uow.Matches.AddAsync(match);
    }

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
        await uow.CommitAsync();

        cache[fixture.CountryCode] = country.Id;
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
        return league.Id;
    }

    private async Task<int> UpsertTeamFromFixtureAsync(int externalId, string name, string? logo, int countryId, Dictionary<int, int> cache)
    {
        if (cache.TryGetValue(externalId, out int cached))
        {
            return cached;
        }

        Team? existing = await uow.Teams.GetByExternalIdAsync(externalId);
        if (existing is not null)
        {
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
        return team.Id;
    }

    private async Task<int?> GetInternalLeagueIdAsync(int externalId, Dictionary<int, int> cache)
    {
        if (cache.TryGetValue(externalId, out int cached))
        {
            return cached;
        }

        League? league = await uow.Leagues.GetByExternalIdAsync(externalId);
        if (league is null)
        {
            return null;
        }

        cache[externalId] = league.Id;
        return league.Id;
    }

    private static int CurrentSeason(FootballApiOptions opts) =>
        opts.SeasonOverride ?? (DateTime.UtcNow.Month >= 7 ? DateTime.UtcNow.Year : DateTime.UtcNow.Year - 1);

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
