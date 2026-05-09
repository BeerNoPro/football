namespace FootballBlog.Core.Options;

public class FootballApiOptions
{
    public const string SectionName = "FootballApi";

    public string BaseUrl { get; set; } = string.Empty;
    public int DailyRequestLimit { get; set; } = 100;
    public int PerMinuteRequestLimit { get; set; } = 10;
    public int[] LeagueIds { get; set; } = [];

    /// <summary>Top leagues nhận full treatment: H2H, squad, AI prediction. Subset của LeagueIds.</summary>
    public int[] PremiumLeagueIds { get; set; } = [];

    public int? SeasonOverride { get; set; }
}
