using FootballBlog.API.Common;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/livescore")]
public class LiveScoreController(ILiveScoreService liveScoreService, ILogger<LiveScoreController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<LiveMatchDto>>>> GetAll()
    {
        var matches = await liveScoreService.GetLiveMatchesAsync();
        return Ok(ApiResponse<IEnumerable<LiveMatchDto>>.Ok(matches));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<LiveMatchDto>>> GetById(int id)
    {
        var match = await liveScoreService.GetMatchByIdAsync(id);
        if (match is null)
        {
            logger.LogWarning("Live match not found: {Id}", id);
            return NotFound(ApiResponse<LiveMatchDto>.Fail($"Live match {id} not found"));
        }
        return Ok(ApiResponse<LiveMatchDto>.Ok(match));
    }
}
