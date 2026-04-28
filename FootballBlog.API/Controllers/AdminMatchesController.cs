using FootballBlog.API.Common;
using FootballBlog.API.Jobs;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/admin/matches")]
[Authorize(Roles = "Admin")]
public class AdminMatchesController(
    ApplicationDbContext dbContext,
    IBackgroundJobClient jobClient,
    ILogger<AdminMatchesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MatchSummaryDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] bool? hasPrediction = null,
        [FromQuery] string? search = null)
    {
        var query = dbContext.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.League)
            .Include(m => m.Prediction)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MatchStatus>(status, true, out var matchStatus))
        {
            query = query.Where(m => m.Status == matchStatus);
        }

        if (hasPrediction.HasValue)
        {
            query = hasPrediction.Value
                ? query.Where(m => m.Prediction != null)
                : query.Where(m => m.Prediction == null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.HomeTeam.Name.Contains(search) || m.AwayTeam.Name.Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.KickoffUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MatchSummaryDto(
                m.Id,
                m.ExternalId,
                m.HomeTeam.Name,
                m.AwayTeam.Name,
                m.League.Name,
                m.Season,
                m.KickoffUtc,
                m.Status,
                m.HomeScore,
                m.AwayScore,
                m.Prediction != null
            ))
            .ToListAsync();

        return Ok(ApiResponse<PagedResult<MatchSummaryDto>>.Ok(new PagedResult<MatchSummaryDto>(items, page, pageSize, total)));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<MatchStatsDto>>> GetStats()
    {
        var total = await dbContext.Matches.CountAsync();
        var live = await dbContext.Matches.CountAsync(m => m.Status == MatchStatus.Live);
        var withPrediction = await dbContext.Matches.CountAsync(m => m.Prediction != null);
        var pending = await dbContext.Matches.CountAsync(m =>
            m.Status == MatchStatus.Scheduled && m.Prediction == null);

        return Ok(ApiResponse<MatchStatsDto>.Ok(new MatchStatsDto(total, live, withPrediction, pending)));
    }

    [HttpPost("fetch")]
    public IActionResult TriggerFetch()
    {
        jobClient.Enqueue<FetchUpcomingMatchesJob>(j => j.ExecuteAsync());
        logger.LogInformation("Admin triggered FetchUpcomingMatchesJob");
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpPost("predict-all")]
    public IActionResult TriggerPredictAll()
    {
        jobClient.Enqueue<GeneratePredictionJob>(j => j.ExecuteAsync());
        logger.LogInformation("Admin triggered GeneratePredictionJob for all pending matches");
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpPost("seed-leagues")]
    public IActionResult TriggerSeedLeagueData()
    {
        jobClient.Enqueue<SeedLeagueDataJob>(j => j.ExecuteAsync(CancellationToken.None));
        logger.LogInformation("Admin triggered SeedLeagueDataJob");
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
