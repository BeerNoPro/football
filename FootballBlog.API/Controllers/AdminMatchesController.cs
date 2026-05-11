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
            .Include(m => m.Predictions)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MatchStatus>(status, true, out var matchStatus))
        {
            query = query.Where(m => m.Status == matchStatus);
        }

        if (hasPrediction.HasValue)
        {
            query = hasPrediction.Value
                ? query.Where(m => m.Predictions.Any(p => p.Phase == PredictionPhase.PreMatch))
                : query.Where(m => !m.Predictions.Any(p => p.Phase == PredictionPhase.PreMatch));
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
                m.Predictions.Any(p => p.Phase == PredictionPhase.PreMatch)
            ))
            .ToListAsync();

        return Ok(ApiResponse<PagedResult<MatchSummaryDto>>.Ok(new PagedResult<MatchSummaryDto>(items, page, pageSize, total)));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<MatchStatsDto>>> GetStats()
    {
        var q = dbContext.Matches.AsNoTracking();
        var total = await q.CountAsync();
        var live = await q.CountAsync(m => m.Status == MatchStatus.Live || m.Status == MatchStatus.HalfTime);
        var withPrediction = await q.CountAsync(m => m.Predictions.Any(p => p.Phase == PredictionPhase.PreMatch));
        var pending = await q.CountAsync(m =>
            m.Status == MatchStatus.Scheduled && !m.Predictions.Any(p => p.Phase == PredictionPhase.PreMatch));

        var bySeasons = await q
            .GroupBy(m => m.Season)
            .Select(g => new { Season = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Season, x => x.Count);

        return Ok(ApiResponse<MatchStatsDto>.Ok(new MatchStatsDto(total, live, withPrediction, pending, bySeasons)));
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

    [HttpPost("trigger-h2h")]
    public async Task<IActionResult> TriggerH2H()
    {
        var now = DateTime.UtcNow;
        var matches = await dbContext.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.ContextData)
            .Where(m => m.Status == MatchStatus.Scheduled && m.KickoffUtc > now && m.ContextData == null)
            .ToListAsync();

        foreach (var match in matches)
        {
            jobClient.Enqueue<PreMatchDataJob>(j => j.FetchH2HAsync(match.ExternalId, match.HomeTeam.ExternalId, match.AwayTeam.ExternalId));
        }

        logger.LogInformation("Admin triggered H2H for {Count} pending matches", matches.Count);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpPost("fetch-squads")]
    public IActionResult TriggerFetchSquads()
    {
        jobClient.Enqueue<FetchSquadJob>(j => j.ExecuteAsync());
        logger.LogInformation("Admin triggered FetchSquadJob");
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpPost("fetch-post-match")]
    public IActionResult TriggerFetchPostMatch()
    {
        jobClient.Enqueue<FetchPostMatchDataJob>(j => j.ExecuteAsync());
        logger.LogInformation("Admin triggered FetchPostMatchDataJob");
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpPost("trigger-telegram")]
    public async Task<IActionResult> TriggerTelegram()
    {
        var predictions = await dbContext.MatchPredictions
            .AsNoTracking()
            .Where(p => p.Phase == PredictionPhase.PreMatch && p.TelegramMessageId == null)
            .ToListAsync();

        foreach (var prediction in predictions)
        {
            jobClient.Enqueue<TelegramNotificationJob>(j => j.SendPredictionAsync(prediction.Id));
        }

        logger.LogInformation("Admin triggered Telegram for {Count} unsent predictions", predictions.Count);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
