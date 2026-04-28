using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public interface IFixtureApiClient
{
    Task<IEnumerable<LeagueDto>?> GetLeaguesAsync();
    Task<PagedResult<FixtureDto>?> GetFixturesAsync(
        int? leagueId = null,
        DateOnly? date = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? status = null,
        string? season = null,
        bool sortAsc = false,
        int page = 1,
        int pageSize = 100);
    Task<IEnumerable<StandingDto>?> GetStandingsAsync(int leagueId, int? season = null);
}
