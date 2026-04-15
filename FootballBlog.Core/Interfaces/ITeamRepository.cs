using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface ITeamRepository : IRepository<Team>
{
    Task<Team?> GetByExternalIdAsync(int externalId);
}
