using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IMatchRepository : IRepository<Match>
{
    Task<Match?> GetByExternalIdAsync(int externalId);

    /// <summary>Trả các trận Scheduled trong N giờ tới — dùng cho Hangfire prediction job.</summary>
    Task<IEnumerable<Match>> GetUpcomingAsync(int hoursAhead = 48);

    /// <summary>Trả các trận chưa có MatchPrediction — dùng để trigger AI.</summary>
    Task<IEnumerable<Match>> GetWithoutPredictionAsync();

    Task<IEnumerable<Match>> GetByStatusAsync(MatchStatus status);

    /// <summary>Lấy match kèm prediction (eager load).</summary>
    Task<Match?> GetWithPredictionAsync(int id);

    /// <summary>5 trận H2H gần nhất giữa 2 đội (home ↔ away cả 2 chiều).</summary>
    Task<IEnumerable<Match>> GetH2HAsync(int homeTeamId, int awayTeamId, int count = 5);

    /// <summary>N trận gần nhất của 1 đội (cả home và away) — dùng để tính form.</summary>
    Task<IEnumerable<Match>> GetRecentByTeamAsync(int teamId, int count = 5);

    /// <summary>Trận Scheduled chưa có ContextData và sắp đấu trong X giờ tới.</summary>
    Task<IEnumerable<Match>> GetWithoutContextAsync(int hoursAhead = 24);
}
