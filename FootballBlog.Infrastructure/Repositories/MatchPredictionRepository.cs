using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class MatchPredictionRepository(ApplicationDbContext dbContext)
    : BaseRepository<MatchPrediction>(dbContext), IMatchPredictionRepository
{
    public async Task<MatchPrediction?> GetByMatchIdAsync(int matchId)
        => await _dbSet
            .AsNoTracking()
            .Include(p => p.Match)
            .FirstOrDefaultAsync(p => p.MatchId == matchId);

    public async Task<IEnumerable<MatchPrediction>> GetUnpublishedAsync()
        => await _dbSet
            .AsNoTracking()
            .Include(p => p.Match)
            .Where(p => !p.IsPublished)
            .OrderBy(p => p.Match.KickoffUtc)
            .ToListAsync();

    public async Task<MatchPrediction?> GetByTelegramMessageIdAsync(long messageId)
        => await _dbSet
            .AsNoTracking()
            .Include(p => p.Match)
            .FirstOrDefaultAsync(p => p.TelegramMessageId == messageId);
}
