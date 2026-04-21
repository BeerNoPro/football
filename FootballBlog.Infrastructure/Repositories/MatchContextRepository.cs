using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class MatchContextRepository : BaseRepository<MatchContextData>, IMatchContextRepository
{
    public MatchContextRepository(ApplicationDbContext context) : base(context) { }

    public async Task<MatchContextData?> GetByMatchIdAsync(int matchId) =>
        await _dbSet.AsNoTracking()
            .TagWithCaller()
            .FirstOrDefaultAsync(c => c.MatchId == matchId);
}
