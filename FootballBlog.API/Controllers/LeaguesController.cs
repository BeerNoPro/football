using FootballBlog.API.Common;
using FootballBlog.Core.DTOs;
using FootballBlog.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/leagues")]
public class LeaguesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeagueDto>>>> GetAll()
    {
        var leagues = await dbContext.Leagues
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Include(l => l.Country)
            .OrderBy(l => l.Country.Name)
            .ThenBy(l => l.Name)
            .Select(l => new LeagueDto(
                l.Id,
                l.ExternalId,
                l.Name,
                l.LogoUrl,
                l.Country.Name,
                l.Country.Code,
                l.Country.FlagUrl
            ))
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<LeagueDto>>.Ok(leagues));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<LeagueDto>>> GetById(int id)
    {
        var league = await dbContext.Leagues
            .AsNoTracking()
            .Include(l => l.Country)
            .Where(l => l.Id == id)
            .Select(l => new LeagueDto(
                l.Id,
                l.ExternalId,
                l.Name,
                l.LogoUrl,
                l.Country.Name,
                l.Country.Code,
                l.Country.FlagUrl
            ))
            .FirstOrDefaultAsync();

        if (league is null)
        {
            return NotFound(ApiResponse<LeagueDto>.Fail($"League {id} not found"));
        }

        return Ok(ApiResponse<LeagueDto>.Ok(league));
    }
}
