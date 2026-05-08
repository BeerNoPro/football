using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IMatchPredictionRepository : IRepository<MatchPrediction>
{
    Task<MatchPrediction?> GetByMatchIdAsync(int matchId);

    Task<MatchPrediction?> GetByMatchAndPhaseAsync(int matchId, PredictionPhase phase);

    Task<MatchPrediction?> GetByTelegramMessageIdAsync(long messageId);
}
