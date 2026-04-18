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
        [FromQuery] bool? isPublished = null)
    {
        var query = dbContext.MatchPredictions
            .AsNoTracking()
            .Include(p => p.Match).ThenInclude(m => m.HomeTeam)
            .Include(p => p.Match).ThenInclude(m => m.AwayTeam)
            .Include(p => p.Match).ThenInclude(m => m.League)
            .AsQueryable();

        if (isPublished.HasValue)
        {
            query = query.Where(p => p.IsPublished == isPublished.Value);
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
                p.IsPublished,
                p.BlogPostId
            ))
            .ToListAsync();

        return Ok(ApiResponse<PagedResult<MatchPredictionDto>>.Ok(new PagedResult<MatchPredictionDto>(items, page, pageSize, total)));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<PredictionStatsDto>>> GetStats()
    {
        var total = await dbContext.MatchPredictions.CountAsync();
        var published = await dbContext.MatchPredictions.CountAsync(p => p.IsPublished);
        var pending = await dbContext.MatchPredictions.CountAsync(p => !p.IsPublished);
        var todayUtc = DateTime.UtcNow.Date;
        var todayCount = await dbContext.MatchPredictions.CountAsync(p => p.GeneratedAt >= todayUtc);

        // Tính accuracy: so sánh PredictedOutcome với kết quả thực (match đã kết thúc)
        var finishedWithPrediction = await dbContext.MatchPredictions
            .Include(p => p.Match)
            .Where(p => p.Match.Status == MatchStatus.Finished
                     && p.Match.HomeScore.HasValue
                     && p.Match.AwayScore.HasValue)
            .Select(p => new
            {
                p.PredictedOutcome,
                ActualOutcome = p.Match.HomeScore > p.Match.AwayScore ? "HomeWin"
                              : p.Match.HomeScore < p.Match.AwayScore ? "AwayWin"
                              : "Draw"
            })
            .ToListAsync();

        decimal accuracy = finishedWithPrediction.Count > 0
            ? Math.Round((decimal)finishedWithPrediction.Count(x => x.PredictedOutcome == x.ActualOutcome)
                / finishedWithPrediction.Count * 100, 1)
            : 0;

        logger.LogDebug("PredictionStats: total={Total}, accuracy={Accuracy}%", total, accuracy);
        return Ok(ApiResponse<PredictionStatsDto>.Ok(new PredictionStatsDto(total, published, pending, todayCount, accuracy)));
    }
}
