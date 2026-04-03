using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IMatchPredictionRepository : IRepository<MatchPrediction>
{
    Task<MatchPrediction?> GetByMatchIdAsync(int matchId);

    /// <summary>Trả các prediction đã generate nhưng chưa publish thành blog post.</summary>
    Task<IEnumerable<MatchPrediction>> GetUnpublishedAsync();

    Task<MatchPrediction?> GetByTelegramMessageIdAsync(long messageId);
}
