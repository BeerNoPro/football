using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IMatchRepository : IRepository<Match>
{
    Task<Match?> GetByExternalIdAsync(int externalId);

    /// <summary>Trả các trận Scheduled trong 48h tới — dùng cho Hangfire prediction job.</summary>
    Task<IEnumerable<Match>> GetUpcomingAsync(int hoursAhead = 48);

    /// <summary>Trả các trận chưa có MatchPrediction — dùng để trigger AI.</summary>
    Task<IEnumerable<Match>> GetWithoutPredictionAsync();

    Task<IEnumerable<Match>> GetByStatusAsync(MatchStatus status);

    /// <summary>Lấy match kèm prediction (eager load).</summary>
    Task<Match?> GetWithPredictionAsync(int id);
}
