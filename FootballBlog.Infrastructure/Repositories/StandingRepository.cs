using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class StandingRepository : BaseRepository<Standing>, IStandingRepository
{
    public StandingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Standing?> GetByLeagueTeamSeasonAsync(int leagueId, int teamId, int season) =>
        await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(s => s.LeagueId == leagueId && s.TeamId == teamId && s.Season == season);

    public async Task<IEnumerable<Standing>> GetByLeagueSeasonAsync(int leagueId, int season) =>
        await _dbSet.AsNoTracking()
            .Include(s => s.Team)
            .Where(s => s.LeagueId == leagueId && s.Season == season)
            .OrderBy(s => s.Rank)
            .ToListAsync();

    public async Task<bool> HasDataForSeasonAsync(int leagueId, int season) =>
        await _dbSet.AsNoTracking()
            .AnyAsync(s => s.LeagueId == leagueId && s.Season == season);
}
