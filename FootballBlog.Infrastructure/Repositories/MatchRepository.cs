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
            .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.League)
            .Where(m => m.Status == MatchStatus.Scheduled && m.KickoffUtc <= cutoff && m.KickoffUtc >= DateTime.UtcNow)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Match>> GetWithoutPredictionAsync()
        => await _dbSet
            .AsNoTracking()
            .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.League)
            .Where(m => m.Status == MatchStatus.Scheduled && m.Prediction == null)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync();

    public async Task<IEnumerable<Match>> GetByStatusAsync(MatchStatus status)
        => await _dbSet
            .AsNoTracking()
            .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.League)
            .Where(m => m.Status == status)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync();

    public async Task<Match?> GetWithPredictionAsync(int id)
        => await _dbSet
            .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.League)
            .Include(m => m.Prediction)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<Match>> GetH2HAsync(int homeTeamId, int awayTeamId, int count = 5)
        => await _dbSet
            .AsNoTracking()
            .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.League)
            .Where(m => m.Status == MatchStatus.Finished &&
                        ((m.HomeTeamId == homeTeamId && m.AwayTeamId == awayTeamId) ||
                         (m.HomeTeamId == awayTeamId && m.AwayTeamId == homeTeamId)))
            .OrderByDescending(m => m.KickoffUtc)
            .Take(count)
            .ToListAsync();

    public async Task<IEnumerable<Match>> GetRecentByTeamAsync(int teamId, int count = 5)
        => await _dbSet
            .AsNoTracking()
            .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.League)
            .Where(m => m.Status == MatchStatus.Finished &&
                        (m.HomeTeamId == teamId || m.AwayTeamId == teamId))
            .OrderByDescending(m => m.KickoffUtc)
            .Take(count)
            .ToListAsync();

    public async Task<IEnumerable<Match>> GetWithoutContextAsync(int hoursAhead = 24)
    {
        var cutoff = DateTime.UtcNow.AddHours(hoursAhead);
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.Status == MatchStatus.Scheduled &&
                        m.KickoffUtc <= cutoff &&
                        m.ContextData == null)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync();
    }
}
