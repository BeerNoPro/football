using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class PlayerRepository : BaseRepository<Player>, IPlayerRepository
{
    public PlayerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Player?> GetByExternalIdAsync(int externalId) =>
        await _dbSet.AsNoTracking()
            .TagWithCaller()
            .FirstOrDefaultAsync(p => p.ExternalId == externalId);
}
