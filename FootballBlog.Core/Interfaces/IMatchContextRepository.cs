using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IMatchContextRepository : IRepository<MatchContextData>
{
    Task<MatchContextData?> GetByMatchIdAsync(int matchId);
}
