using System.Text.Json;
using System.Text.Json.Serialization;

namespace FootballBlog.API.ApiClients.FootballApi;

internal record FootballApiEnvelope<T>(
    [property: JsonPropertyName("response")] T[] Response,
    // Football API trả [] khi không có lỗi, object {"key":"msg"} khi có lỗi
    [property: JsonPropertyName("errors")] JsonElement Errors);

internal record FixtureResponse(
    [property: JsonPropertyName("fixture")] FixtureInfo Fixture,
    [property: JsonPropertyName("league")] LeagueInfo League,
    [property: JsonPropertyName("teams")] TeamsInfo Teams,
    [property: JsonPropertyName("goals")] GoalsInfo Goals);

internal record FixtureInfo(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("date")] DateTimeOffset Date,
    [property: JsonPropertyName("status")] FixtureStatus Status,
    [property: JsonPropertyName("venue")] VenueInfo? Venue,
    [property: JsonPropertyName("referee")] string? Referee);

internal record FixtureStatus(
    [property: JsonPropertyName("short")] string Short,
    [property: JsonPropertyName("elapsed")] int? Elapsed);

internal record LeagueInfo(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("logo")] string? Logo,
    [property: JsonPropertyName("flag")] string? Flag,
    [property: JsonPropertyName("season")] int Season,
    [property: JsonPropertyName("round")] string? Round);

internal record TeamsInfo(
    [property: JsonPropertyName("home")] TeamInfo Home,
    [property: JsonPropertyName("away")] TeamInfo Away);

internal record TeamInfo(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("logo")] string? Logo);

internal record GoalsInfo(
    [property: JsonPropertyName("home")] int? Home,
    [property: JsonPropertyName("away")] int? Away);

internal record VenueInfo(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("city")] string? City);

// ── GET /teams?league=X&season=Y ────────────────────────────────────────────

internal record TeamResponse(
    [property: JsonPropertyName("team")] TeamDetail Team,
    [property: JsonPropertyName("venue")] VenueDetail? Venue);

internal record TeamDetail(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("logo")] string? Logo);

internal record VenueDetail(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("capacity")] int? Capacity,
    [property: JsonPropertyName("image")] string? Image);

// ── GET /standings?league=X&season=Y ────────────────────────────────────────

internal record StandingsEnvelope(
    [property: JsonPropertyName("league")] StandingsLeague League);

internal record StandingsLeague(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("season")] int Season,
    [property: JsonPropertyName("standings")] StandingEntry[][] Standings);

internal record StandingEntry(
    [property: JsonPropertyName("rank")] int Rank,
    [property: JsonPropertyName("team")] TeamInfo Team,
    [property: JsonPropertyName("points")] int Points,
    [property: JsonPropertyName("goalsDiff")] int GoalsDiff,
    [property: JsonPropertyName("form")] string? Form,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("all")] StandingStats All,
    [property: JsonPropertyName("update")] DateTimeOffset UpdatedAt);

internal record StandingStats(
    [property: JsonPropertyName("played")] int Played,
    [property: JsonPropertyName("win")] int Won,
    [property: JsonPropertyName("draw")] int Drawn,
    [property: JsonPropertyName("lose")] int Lost,
    [property: JsonPropertyName("goals")] StandingGoals Goals);

internal record StandingGoals(
    [property: JsonPropertyName("for")] int For,
    [property: JsonPropertyName("against")] int Against);
