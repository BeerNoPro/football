using System.Net.Http.Json;
using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public class PostApiClient(HttpClient httpClient, ILogger<PostApiClient> logger) : IPostApiClient
{
    // Record nội bộ để deserialize wrapper từ API
    private record ApiResponse<T>(bool Success, T? Data, string? Error);

    public async Task<PagedResult<PostSummaryDto>?> GetPublishedAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<PostSummaryDto>>>(
                $"api/posts?page={page}&pageSize={pageSize}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch published posts page={Page}", page);
            return null;
        }
    }

    public async Task<PostDetailDto?> GetBySlugAsync(string slug)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<PostDetailDto>>(
                $"api/posts/{slug}");
            return response?.Data;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch post by slug {Slug}", slug);
            return null;
        }
    }

    public async Task<PagedResult<PostSummaryDto>?> GetByCategoryAsync(string categorySlug, int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<PostSummaryDto>>>(
                $"api/posts/by-category/{categorySlug}?page={page}&pageSize={pageSize}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch posts by category {CategorySlug}", categorySlug);
            return null;
        }
    }

    public async Task<PagedResult<PostSummaryDto>?> GetByTagAsync(string tagSlug, int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<PostSummaryDto>>>(
                $"api/posts/by-tag/{tagSlug}?page={page}&pageSize={pageSize}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch posts by tag {TagSlug}", tagSlug);
            return null;
        }
    }
}
