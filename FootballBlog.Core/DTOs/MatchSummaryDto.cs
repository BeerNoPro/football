using FootballBlog.Core.Models;

namespace FootballBlog.Core.DTOs;

public record MatchSummaryDto(
    int Id,
    int ExternalId,
    string HomeTeam,
    string AwayTeam,
    string LeagueName,
    string Season,
    DateTime KickoffUtc,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore,
    bool HasPrediction
);
