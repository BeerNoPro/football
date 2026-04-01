using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class LiveMatchRepository : BaseRepository<LiveMatch>, ILiveMatchRepository
{
    public LiveMatchRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<IEnumerable<LiveMatch>> GetLiveMatchesAsync() =>
        await _dbSet
            .Where(m => m.Status == "LIVE")
            .Include(m => m.Events)
            .OrderBy(m => m.StartedAt)
            .ToListAsync();

    public async Task<LiveMatch?> GetByExternalIdAsync(int externalId) =>
        await _dbSet.FirstOrDefaultAsync(m => m.ExternalId == externalId);

    public async Task<IEnumerable<LiveMatch>> GetByStatusAsync(string status) =>
        await _dbSet
            .Where(m => m.Status == status)
            .OrderBy(m => m.StartedAt)
            .ToListAsync();
}
