using System.Net.Http.Json;
using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public class CategoryApiClient(HttpClient httpClient, ILogger<CategoryApiClient> logger) : ICategoryApiClient
{
    private record ApiResponse<T>(bool Success, T? Data, string? Error);

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<CategoryDto>>>("api/categories");
            return response?.Data ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch all categories");
            return [];
        }
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<CategoryDto>>($"api/categories/{slug}");
            return response?.Data;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch category by slug {Slug}", slug);
            return null;
        }
    }
}
