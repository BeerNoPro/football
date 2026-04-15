using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface ILeagueRepository : IRepository<League>
{
    Task<League?> GetByExternalIdAsync(int externalId);

    /// <summary>Trả danh sách giải đấu đang active — dùng cho FetchUpcomingMatchesJob.</summary>
    Task<IEnumerable<League>> GetActiveAsync();
}
