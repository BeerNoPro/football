using System.Net.Http.Json;
using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public class LiveScoreApiClient(HttpClient httpClient, ILogger<LiveScoreApiClient> logger) : ILiveScoreApiClient
{
    private record ApiResponse<T>(bool Success, T? Data, string? Error);

    public async Task<IEnumerable<LiveMatchDto>?> GetLiveMatchesAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<LiveMatchDto>>>("api/livescore");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch live matches");
            return null;
        }
    }

    public async Task<LiveMatchDto?> GetMatchByIdAsync(int id)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<LiveMatchDto>>($"api/livescore/{id}");
            return response?.Data;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch live match {Id}", id);
            return null;
        }
    }
}
