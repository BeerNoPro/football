using FootballBlog.API.Common;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/admin/predictions")]
[Authorize(Roles = "Admin")]
public class AdminPredictionsController(
    ApplicationDbContext dbContext,
    ILogger<AdminPredictionsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MatchPredictionDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? phase = null)
    {
        var query = dbContext.MatchPredictions
            .AsNoTracking()
            .Include(p => p.Match).ThenInclude(m => m.HomeTeam)
            .Include(p => p.Match).ThenInclude(m => m.AwayTeam)
            .Include(p => p.Match).ThenInclude(m => m.League)
            .AsQueryable();

        if (phase == "prematch")
        {
            query = query.Where(p => p.Phase == PredictionPhase.PreMatch);
        }
        else if (phase == "halftime")
        {
            query = query.Where(p => p.Phase == PredictionPhase.HalfTime);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.GeneratedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new MatchPredictionDto(
                p.Id,
                p.MatchId,
                p.Match.HomeTeam.Name,
                p.Match.AwayTeam.Name,
                p.Match.League.Name,
                p.Match.KickoffUtc,
                p.AIProvider,
                p.AIModel,
                p.PredictedHomeScore,
                p.PredictedAwayScore,
                p.PredictedOutcome,
                p.ConfidenceScore,
                p.AnalysisSummary,
                p.GeneratedAt,
                p.Phase.ToString(),
                p.TelegramMessageId != null
            ))
            .ToListAsync();

        return Ok(ApiResponse<PagedResult<MatchPredictionDto>>.Ok(new PagedResult<MatchPredictionDto>(items, page, pageSize, total)));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<PredictionStatsDto>>> GetStats()
    {
        var total = await dbContext.MatchPredictions.TagWithCaller().CountAsync();
        var telegramSent = await dbContext.MatchPredictions
            .TagWithCaller()
            .CountAsync(p => p.Phase == PredictionPhase.PreMatch && p.TelegramMessageId != null);
        var todayUtc = DateTime.UtcNow.Date;
        var todayCount = await dbContext.MatchPredictions
            .TagWithCaller()
            .CountAsync(p => p.GeneratedAt >= todayUtc);

        var finishedWithPrediction = await dbContext.MatchPredictions
            .AsNoTracking()
            .Include(p => p.Match)
            .Where(p => p.Phase == PredictionPhase.PreMatch
                     && p.Match.Status == MatchStatus.Finished
                     && p.Match.HomeScore.HasValue
                     && p.Match.AwayScore.HasValue)
            .Select(p => new
            {
                p.PredictedOutcome,
                ActualOutcome = p.Match.HomeScore > p.Match.AwayScore ? "HomeWin"
                              : p.Match.HomeScore < p.Match.AwayScore ? "AwayWin"
                              : "Draw"
            })
            .TagWithCaller()
            .ToListAsync();

        decimal accuracy = finishedWithPrediction.Count > 0
            ? Math.Round((decimal)finishedWithPrediction.Count(x => x.PredictedOutcome == x.ActualOutcome)
                / finishedWithPrediction.Count * 100, 1)
            : 0;

        logger.LogDebug("PredictionStats: total={Total}, telegramSent={TelegramSent}, accuracy={Accuracy}%", total, telegramSent, accuracy);
        return Ok(ApiResponse<PredictionStatsDto>.Ok(new PredictionStatsDto(total, telegramSent, todayCount, accuracy)));
    }
}
