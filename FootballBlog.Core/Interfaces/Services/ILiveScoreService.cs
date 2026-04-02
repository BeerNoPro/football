using FootballBlog.Core.DTOs;

namespace FootballBlog.Core.Interfaces.Services;

/// <summary>
/// Interface cho live score service. Polling logic sẽ được implement ở Phase 4.
/// </summary>
public interface ILiveScoreService
{
    Task<IEnumerable<LiveMatchDto>> GetLiveMatchesAsync();
    Task<LiveMatchDto?> GetMatchByIdAsync(int id);
}
