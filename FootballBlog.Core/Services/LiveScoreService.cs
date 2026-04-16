using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Logging;

namespace FootballBlog.Core.Services;

public class LiveScoreService(IUnitOfWork uow, ILogger<LiveScoreService> logger) : ILiveScoreService
{
    public async Task<IEnumerable<LiveMatchDto>> GetLiveMatchesAsync()
    {
        logger.LogDebug("Getting live matches");

        var matches = await uow.LiveMatches.GetLiveMatchesAsync();
        var result = matches.Select(ToDto).ToList();

        logger.LogInformation("Live matches retrieved: {Count}", result.Count);
        return result;
    }

    public async Task<LiveMatchDto?> GetMatchByIdAsync(int id)
    {
        logger.LogDebug("Getting live match {Id}", id);

        var match = await uow.LiveMatches.GetByIdAsync(id);
        if (match is null)
        {
            logger.LogWarning("Live match not found: {Id}", id);
            return null;
        }

        return ToDto(match);
    }

    private static LiveMatchDto ToDto(LiveMatch m) => new(
        m.Id,
        m.ExternalId,
        m.HomeTeam,
        m.AwayTeam,
        m.HomeScore,
        m.AwayScore,
        m.Status.ToString(),
        m.Minute,
        m.StartedAt,
        m.Events.Select(e => new MatchEventDto(e.Id, e.Minute, e.Type.ToString(), e.Description)).ToList()
    );
}
