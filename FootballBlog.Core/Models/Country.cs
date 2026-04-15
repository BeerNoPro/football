namespace FootballBlog.Core.Models;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // "England", "Vietnam"
    public string Code { get; set; } = string.Empty;   // "GB-ENG", "VN" (ISO)
    public string? FlagUrl { get; set; }

    public ICollection<League> Leagues { get; set; } = [];
    public ICollection<Team> Teams { get; set; } = [];
}
