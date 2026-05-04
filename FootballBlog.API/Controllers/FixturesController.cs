using FootballBlog.API.Common;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Models;
using FootballBlog.Core.Options;
using FootballBlog.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/fixtures")]
public class FixturesController(ApplicationDbContext dbContext, IOptions<FootballApiOptions> footballOptions) : ControllerBase
{
    // GET /api/fixtures?leagueId=1&date=2024-12-01&fromDate=2024-12-01&toDate=2024-12-07&status=Finished&sortAsc=true&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FixtureDto>>>> GetAll(
        [FromQuery] int? leagueId = null,
        [FromQuery] DateOnly? date = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? status = null,
        [FromQuery] string? season = null,
        [FromQuery] string? search = null,
        [FromQuery] bool sortAsc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Default về mùa giải hiện tại nếu không truyền season (tránh trả data cũ lẫn lộn)
        string effectiveSeason = season ?? CurrentSeason();

        var query = dbContext.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.League).ThenInclude(l => l.Country)
            .Include(m => m.Prediction)
            .Where(m => m.Season == effectiveSeason)
            .AsQueryable();

        if (leagueId.HasValue)
        {
            query = query.Where(m => m.LeagueId == leagueId.Value);
        }

        if (date.HasValue)
        {
            var start = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end = start.AddDays(1);
            query = query.Where(m => m.KickoffUtc >= start && m.KickoffUtc < end);
        }
        else
        {
            if (fromDate.HasValue)
            {
                var start = fromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                query = query.Where(m => m.KickoffUtc >= start);
            }
            if (toDate.HasValue)
            {
                var end = toDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
                query = query.Where(m => m.KickoffUtc < end);
            }
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MatchStatus>(status, true, out var matchStatus))
        {
            query = query.Where(m => m.Status == matchStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m =>
                m.HomeTeam.Name.Contains(search) || m.AwayTeam.Name.Contains(search));
        }

        var total = await query.CountAsync();
        var ordered = sortAsc
            ? query.OrderBy(m => m.KickoffUtc)
            : query.OrderByDescending(m => m.KickoffUtc);
        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new FixtureDto(
                m.Id,
                m.ExternalId,
                m.LeagueId,
                m.League.Name,
                m.League.LogoUrl,
                m.League.Country.Name,
                m.League.Country.FlagUrl,
                m.Season,
                m.Round,
                m.KickoffUtc,
                m.Status,
                m.HomeScore,
                m.AwayScore,
                m.HomeTeamId,
                m.HomeTeam.Name,
                m.HomeTeam.LogoUrl,
                m.AwayTeamId,
                m.AwayTeam.Name,
                m.AwayTeam.LogoUrl,
                m.VenueName,
                m.Prediction != null
            ))
            .ToListAsync();

        return Ok(ApiResponse<PagedResult<FixtureDto>>.Ok(
            new PagedResult<FixtureDto>(items, page, pageSize, total)));
    }

    // GET /api/fixtures/suggest?q=man&limit=6
    [HttpGet("suggest")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FixtureSuggestDto>>>> Suggest(
        [FromQuery] string q = "",
        [FromQuery] int limit = 6)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Ok(ApiResponse<IEnumerable<FixtureSuggestDto>>.Ok([]));
        }

        var lower = q.ToLower();
        string season = CurrentSeason();

        var matches = await dbContext.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.Season == season &&
                        (m.HomeTeam.Name.ToLower().Contains(lower) ||
                         m.AwayTeam.Name.ToLower().Contains(lower)))
            .Select(m => new { m.Id, Home = m.HomeTeam.Name, Away = m.AwayTeam.Name })
            .ToListAsync();

        var ranked = matches
            .OrderByDescending(m => m.Home.ToLower().StartsWith(lower) || m.Away.ToLower().StartsWith(lower))
            .ThenBy(m => m.Home)
            .Take(limit)
            .Select(m => new FixtureSuggestDto(m.Id, m.Home, m.Away));

        return Ok(ApiResponse<IEnumerable<FixtureSuggestDto>>.Ok(ranked));
    }

    // GET /api/fixtures/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<FixtureDto>>> GetById(int id)
    {
        var fixture = await dbContext.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.League).ThenInclude(l => l.Country)
            .Include(m => m.Prediction)
            .Where(m => m.Id == id)
            .Select(m => new FixtureDto(
                m.Id,
                m.ExternalId,
                m.LeagueId,
                m.League.Name,
                m.League.LogoUrl,
                m.League.Country.Name,
                m.League.Country.FlagUrl,
                m.Season,
                m.Round,
                m.KickoffUtc,
                m.Status,
                m.HomeScore,
                m.AwayScore,
                m.HomeTeamId,
                m.HomeTeam.Name,
                m.HomeTeam.LogoUrl,
                m.AwayTeamId,
                m.AwayTeam.Name,
                m.AwayTeam.LogoUrl,
                m.VenueName,
                m.Prediction != null
            ))
            .FirstOrDefaultAsync();

        if (fixture is null)
        {
            return NotFound(ApiResponse<FixtureDto>.Fail($"Fixture {id} not found"));
        }

        return Ok(ApiResponse<FixtureDto>.Ok(fixture));
    }

    // GET /api/fixtures/standings/{leagueId}?season=2024
    [HttpGet("standings/{leagueId:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StandingDto>>>> GetStandings(
        int leagueId,
        [FromQuery] int? season = null)
    {
        var seasonFilter = season ?? await dbContext.Standings
            .Where(s => s.LeagueId == leagueId)
            .MaxAsync(s => (int?)s.Season) ?? 0;

        var standings = await dbContext.Standings
            .AsNoTracking()
            .Include(s => s.Team)
            .Where(s => s.LeagueId == leagueId && s.Season == seasonFilter)
            .OrderBy(s => s.Rank)
            .Select(s => new StandingDto(
                s.Rank,
                s.TeamId,
                s.Team.Name,
                s.Team.LogoUrl,
                s.Points,
                s.Played,
                s.Won,
                s.Drawn,
                s.Lost,
                s.GoalsFor,
                s.GoalsAgainst,
                s.GoalsDiff,
                s.Form,
                s.Description,
                s.Status
            ))
            .ToListAsync();

        if (standings.Count == 0)
        {
            return NotFound(ApiResponse<IEnumerable<StandingDto>>.Fail(
                $"No standings found for league {leagueId} season {seasonFilter}"));
        }

        return Ok(ApiResponse<IEnumerable<StandingDto>>.Ok(standings));
    }

    private string CurrentSeason()
    {
        FootballApiOptions opts = footballOptions.Value;
        int season = opts.SeasonOverride ?? (DateTime.UtcNow.Month >= 7 ? DateTime.UtcNow.Year : DateTime.UtcNow.Year - 1);
        return season.ToString();
    }
}
