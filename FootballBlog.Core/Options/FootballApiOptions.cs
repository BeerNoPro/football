namespace FootballBlog.Core.Options;

public class FootballApiOptions
{
    public const string SectionName = "FootballApi";

    public string BaseUrl { get; set; } = string.Empty;
    public int DailyRequestLimit { get; set; } = 100;
    public int FixturesPerLeague { get; set; } = 20;
    public int[] LeagueIds { get; set; } = [];
    public int? SeasonOverride { get; set; }
}
