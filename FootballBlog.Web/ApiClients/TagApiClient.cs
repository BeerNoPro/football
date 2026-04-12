using System.Net.Http.Json;
using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public class TagApiClient(HttpClient httpClient, ILogger<TagApiClient> logger) : ITagApiClient
{
    private record ApiResponse<T>(bool Success, T? Data, string? Error);

    public async Task<IEnumerable<TagDto>> GetAllAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<TagDto>>>("api/tags");
            return response?.Data ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch all tags");
            return [];
        }
    }

    public async Task<TagDto?> GetBySlugAsync(string slug)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<TagDto>>($"api/tags/{slug}");
            return response?.Data;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch tag by slug {Slug}", slug);
            return null;
        }
    }

    public async Task<PagedResult<PostSummaryDto>?> GetPostsAsync(string slug, int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<PostSummaryDto>>>(
                $"api/tags/{slug}/posts?page={page}&pageSize={pageSize}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch posts for tag {Slug}", slug);
            return null;
        }
    }
}
