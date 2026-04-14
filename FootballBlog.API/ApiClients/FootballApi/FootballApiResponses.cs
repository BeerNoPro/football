using System.Text.Json.Serialization;

namespace FootballBlog.API.ApiClients.FootballApi;

internal record FootballApiEnvelope<T>(
    [property: JsonPropertyName("response")] T[] Response);

internal record FixtureResponse(
    [property: JsonPropertyName("fixture")] FixtureInfo Fixture,
    [property: JsonPropertyName("league")] LeagueInfo League,
    [property: JsonPropertyName("teams")] TeamsInfo Teams,
    [property: JsonPropertyName("goals")] GoalsInfo Goals);

internal record FixtureInfo(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("status")] FixtureStatus Status,
    [property: JsonPropertyName("venue")] VenueInfo? Venue,
    [property: JsonPropertyName("referee")] string? Referee);

internal record FixtureStatus(
    [property: JsonPropertyName("short")] string Short,
    [property: JsonPropertyName("elapsed")] int? Elapsed);

internal record LeagueInfo(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("season")] int Season,
    [property: JsonPropertyName("round")] string? Round);

internal record TeamsInfo(
    [property: JsonPropertyName("home")] TeamInfo Home,
    [property: JsonPropertyName("away")] TeamInfo Away);

internal record TeamInfo(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name);

internal record GoalsInfo(
    [property: JsonPropertyName("home")] int? Home,
    [property: JsonPropertyName("away")] int? Away);

internal record VenueInfo(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("city")] string? City);
