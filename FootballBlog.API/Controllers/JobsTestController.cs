using FootballBlog.API.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballBlog.API.Controllers;

/// <summary>Test controller để manually trigger jobs (chỉ dùng trong development)</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class JobsTestController(
    IBackgroundJobClient backgroundJobs,
    ILogger<JobsTestController> logger) : ControllerBase
{
    /// <summary>Manually trigger FetchUpcomingMatchesJob</summary>
    [HttpPost("trigger/fetch-matches")]
    public IActionResult TriggerFetchMatches()
    {
        logger.LogInformation("Admin triggered FetchUpcomingMatchesJob manually");
        var jobId = backgroundJobs.Enqueue<FetchUpcomingMatchesJob>(j => j.ExecuteAsync());
        return Ok(new { jobId, message = "FetchUpcomingMatchesJob enqueued" });
    }

    /// <summary>Manually trigger LiveScorePollingJob</summary>
    [HttpPost("trigger/live-score")]
    public IActionResult TriggerLiveScore()
    {
        logger.LogInformation("Admin triggered LiveScorePollingJob manually");
        var jobId = backgroundJobs.Enqueue<LiveScorePollingJob>(j => j.ExecuteAsync());
        return Ok(new { jobId, message = "LiveScorePollingJob enqueued" });
    }

    /// <summary>Manually trigger GeneratePredictionJob</summary>
    [HttpPost("trigger/generate-predictions")]
    public IActionResult TriggerGeneratePredictions()
    {
        logger.LogInformation("Admin triggered GeneratePredictionJob manually");
        var jobId = backgroundJobs.Enqueue<GeneratePredictionJob>(j => j.ExecuteAsync());
        return Ok(new { jobId, message = "GeneratePredictionJob enqueued" });
    }
}
