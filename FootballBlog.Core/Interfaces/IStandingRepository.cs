using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IStandingRepository : IRepository<Standing>
{
    Task<Standing?> GetByLeagueTeamSeasonAsync(int leagueId, int teamId, int season);
    Task<IEnumerable<Standing>> GetByLeagueSeasonAsync(int leagueId, int season);
    Task<bool> HasDataForSeasonAsync(int leagueId, int season);
}
