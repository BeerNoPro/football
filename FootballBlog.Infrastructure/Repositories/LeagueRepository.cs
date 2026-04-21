using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class LeagueRepository : BaseRepository<League>, ILeagueRepository
{
    public LeagueRepository(ApplicationDbContext context) : base(context) { }

    public async Task<League?> GetByExternalIdAsync(int externalId) =>
        await _dbSet.AsNoTracking()
            .TagWithCaller()
            .FirstOrDefaultAsync(l => l.ExternalId == externalId);

    public async Task<IEnumerable<League>> GetActiveAsync() =>
        await _dbSet.AsNoTracking()
            .Where(l => l.IsActive)
            .TagWithCaller()
            .ToListAsync();
}
