using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface ISquadMemberRepository : IRepository<SquadMember>
{
    Task<SquadMember?> GetByTeamAndPlayerAsync(int teamId, int playerId);
    Task<IEnumerable<SquadMember>> GetByTeamIdAsync(int teamId);
    Task<bool> HasSquadAsync(int teamId);
}
