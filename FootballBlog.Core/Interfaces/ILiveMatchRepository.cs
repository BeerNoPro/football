using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface ILiveMatchRepository : IRepository<LiveMatch>
{
    Task<IEnumerable<LiveMatch>> GetLiveMatchesAsync();
    Task<LiveMatch?> GetByExternalIdAsync(int externalId);
    Task<IEnumerable<LiveMatch>> GetByStatusAsync(string status);
}
