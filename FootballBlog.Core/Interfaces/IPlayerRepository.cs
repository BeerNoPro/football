using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IPlayerRepository : IRepository<Player>
{
    Task<Player?> GetByExternalIdAsync(int externalId);
}
