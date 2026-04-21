using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class TeamRepository : BaseRepository<Team>, ITeamRepository
{
    public TeamRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Team?> GetByExternalIdAsync(int externalId) =>
        await _dbSet.AsNoTracking()
            .TagWithCaller()
            .FirstOrDefaultAsync(t => t.ExternalId == externalId);
}
