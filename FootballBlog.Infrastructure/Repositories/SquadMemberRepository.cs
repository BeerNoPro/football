using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class SquadMemberRepository : BaseRepository<SquadMember>, ISquadMemberRepository
{
    public SquadMemberRepository(ApplicationDbContext context) : base(context) { }

    public async Task<SquadMember?> GetByTeamAndPlayerAsync(int teamId, int playerId) =>
        await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TeamId == teamId && s.PlayerId == playerId);

    public async Task<IEnumerable<SquadMember>> GetByTeamIdAsync(int teamId) =>
        await _dbSet.AsNoTracking()
            .Include(s => s.Player)
            .Where(s => s.TeamId == teamId)
            .OrderBy(s => s.Number)
            .ToListAsync();

    public async Task<bool> HasSquadAsync(int teamId) =>
        await _dbSet.AsNoTracking().AnyAsync(s => s.TeamId == teamId);
}
