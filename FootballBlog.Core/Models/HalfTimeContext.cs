namespace FootballBlog.Core.Models;

public record HalfTimeContext(
    int HtHomeScore,
    int HtAwayScore,
    string? HomePossession,
    string? AwayPossession,
    int? HomeShotsOnTarget,
    int? AwayShotsOnTarget,
    int? HomeCorners,
    int? AwayCorners,
    int? HomeYellowCards,
    int? AwayYellowCards,
    List<HalfTimeEvent> H1Events
);

public record HalfTimeEvent(int Minute, string Type, string Team, string? PlayerName);
