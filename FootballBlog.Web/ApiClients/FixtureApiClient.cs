using System.Net.Http.Json;
using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public class FixtureApiClient(HttpClient httpClient, ILogger<FixtureApiClient> logger) : IFixtureApiClient
{
    private record ApiResponse<T>(bool Success, T? Data, string? Error);

    public async Task<IEnumerable<LeagueDto>?> GetLeaguesAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<LeagueDto>>>("api/leagues");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch leagues");
            return null;
        }
    }

    public async Task<PagedResult<FixtureDto>?> GetFixturesAsync(
        int? leagueId = null,
        DateOnly? date = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? status = null,
        string? season = null,
        bool sortAsc = false,
        int page = 1,
        int pageSize = 100)
    {
        try
        {
            var qs = new List<string> { $"page={page}", $"pageSize={pageSize}" };

            if (leagueId.HasValue)
            {
                qs.Add($"leagueId={leagueId}");
            }

            if (date.HasValue)
            {
                qs.Add($"date={date.Value:yyyy-MM-dd}");
            }

            if (fromDate.HasValue)
            {
                qs.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            }

            if (toDate.HasValue)
            {
                qs.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            }

            if (!string.IsNullOrEmpty(status))
            {
                qs.Add($"status={status}");
            }

            if (!string.IsNullOrEmpty(season))
            {
                qs.Add($"season={season}");
            }

            if (sortAsc)
            {
                qs.Add("sortAsc=true");
            }

            var response = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<FixtureDto>>>(
                $"api/fixtures?{string.Join("&", qs)}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch fixtures leagueId={LeagueId} date={Date}", leagueId, date ?? fromDate);
            return null;
        }
    }

    public async Task<IEnumerable<StandingDto>?> GetStandingsAsync(int leagueId, int? season = null)
    {
        try
        {
            var url = $"api/fixtures/standings/{leagueId}";
            if (season.HasValue)
            {
                url += $"?season={season}";
            }

            var response = await httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<StandingDto>>>(url);
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch standings leagueId={LeagueId}", leagueId);
            return null;
        }
    }
}
