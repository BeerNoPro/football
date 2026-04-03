using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class MatchRepository(ApplicationDbContext dbContext) : BaseRepository<Match>(dbContext), IMatchRepository
{
    public async Task<Match?> GetByExternalIdAsync(int externalId)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(m => m.ExternalId == externalId);

    public async Task<IEnumerable<Match>> GetUpcomingAsync(int hoursAhead = 48)
    {
        var cutoff = DateTime.UtcNow.AddHours(hoursAhead);
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.Status == MatchStatus.Scheduled && m.KickoffUtc <= cutoff && m.KickoffUtc >= DateTime.UtcNow)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Match>> GetWithoutPredictionAsync()
        => await _dbSet
            .AsNoTracking()
            .Where(m => m.Status == MatchStatus.Scheduled && m.Prediction == null)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync();

    public async Task<IEnumerable<Match>> GetByStatusAsync(MatchStatus status)
        => await _dbSet
            .AsNoTracking()
            .Where(m => m.Status == status)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync();

    public async Task<Match?> GetWithPredictionAsync(int id)
        => await _dbSet
            .Include(m => m.Prediction)
            .FirstOrDefaultAsync(m => m.Id == id);
}
