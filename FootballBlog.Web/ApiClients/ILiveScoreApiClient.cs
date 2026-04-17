using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public interface ILiveScoreApiClient
{
    Task<IEnumerable<LiveMatchDto>?> GetLiveMatchesAsync();
    Task<LiveMatchDto?> GetMatchByIdAsync(int id);
}
