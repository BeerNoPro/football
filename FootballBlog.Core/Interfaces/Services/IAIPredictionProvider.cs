using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces.Services;

public record AIPredictionResult(
    string PredictedOutcome,
    int? PredictedHomeScore,
    int? PredictedAwayScore,
    decimal ConfidenceScore,
    string AnalysisSummary,
    int PromptTokens,
    int CompletionTokens
);

public interface IAIPredictionProvider
{
    string ProviderName { get; }
    string ModelName { get; }
    Task<AIPredictionResult> PredictAsync(Match match, MatchContext context, CancellationToken ct = default);
}
