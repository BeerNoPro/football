using System.Net.Http.Json;
using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public class AdminApiClient(HttpClient httpClient, ILogger<AdminApiClient> logger) : IAdminApiClient
{
    private record ApiResponse<T>(bool Success, T? Data, string? Error);

    public async Task<IEnumerable<ApiKeyDto>?> GetApiKeysAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<ApiKeyDto>>>("api/admin/api-keys");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch API keys");
            return null;
        }
    }

    public async Task<ApiKeyDto?> CreateApiKeyAsync(CreateApiKeyDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/admin/api-keys", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ApiKeyDto>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to create API key for {Provider}", dto.Provider);
            return null;
        }
    }

    public async Task<ApiKeyDto?> ToggleApiKeyAsync(int id)
    {
        try
        {
            var response = await httpClient.PatchAsync($"api/admin/api-keys/{id}/toggle", null);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ApiKeyDto>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to toggle API key {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteApiKeyAsync(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/admin/api-keys/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to delete API key {Id}", id);
            return false;
        }
    }

    public async Task<PagedResult<PostSummaryDto>?> GetAllPostsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<PostSummaryDto>>>(
                $"api/posts/all?page={page}&pageSize={pageSize}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch all posts");
            return null;
        }
    }

    public async Task<PostDetailDto?> GetPostByIdAsync(int id)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<PostDetailDto>>($"api/posts/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch post {Id}", id);
            return null;
        }
    }

    public async Task<PostDetailDto?> CreatePostAsync(CreatePostDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/posts", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PostDetailDto>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to create post {Title}", dto.Title);
            return null;
        }
    }

    public async Task<PostDetailDto?> UpdatePostAsync(int id, CreatePostDto dto)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/posts/{id}", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PostDetailDto>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to update post {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/posts/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to delete post {Id}", id);
            return false;
        }
    }

    public async Task<string?> UploadImageAsync(Stream stream, string fileName, string contentType)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", fileName);

            var response = await httpClient.PostAsync("api/media/upload", content);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to upload image {FileName}", fileName);
            return null;
        }
    }

    public async Task<IEnumerable<CategoryDto>?> GetCategoriesAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<CategoryDto>>>("api/categories");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch categories");
            return null;
        }
    }

    public async Task<CategoryDto?> CreateCategoryAsync(string name, string slug)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/categories", new { name, slug });
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to create category {Name}", name);
            return null;
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/categories/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to delete category {Id}", id);
            return false;
        }
    }

    public async Task<IEnumerable<TagDto>?> GetTagsAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<TagDto>>>("api/tags");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch tags");
            return null;
        }
    }

    public async Task<bool> DeleteTagAsync(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/tags/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to delete tag {Id}", id);
            return false;
        }
    }

    public async Task<PagedResult<MatchSummaryDto>?> GetMatchesAsync(int page = 1, int pageSize = 20, string? status = null, bool? hasPrediction = null, string? search = null)
    {
        try
        {
            var url = $"api/admin/matches?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(status))
            {
                url += $"&status={status}";
            }

            if (hasPrediction.HasValue)
            {
                url += $"&hasPrediction={hasPrediction.Value}";
            }

            if (!string.IsNullOrEmpty(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            var response = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<MatchSummaryDto>>>(url);
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch matches");
            return null;
        }
    }

    public async Task<MatchStatsDto?> GetMatchStatsAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<MatchStatsDto>>("api/admin/matches/stats");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch match stats");
            return null;
        }
    }

    public async Task<bool> TriggerFetchMatchesAsync()
    {
        try
        {
            var response = await httpClient.PostAsync("api/admin/matches/fetch", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to trigger fetch matches");
            return false;
        }
    }

    public async Task<bool> TriggerPredictAllAsync()
    {
        try
        {
            var response = await httpClient.PostAsync("api/admin/matches/predict-all", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to trigger predict all");
            return false;
        }
    }

    public async Task<PagedResult<MatchPredictionDto>?> GetPredictionsAsync(int page = 1, int pageSize = 20, bool? isPublished = null)
    {
        try
        {
            var url = $"api/admin/predictions?page={page}&pageSize={pageSize}";
            if (isPublished.HasValue)
            {
                url += $"&isPublished={isPublished.Value}";
            }

            var response = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<MatchPredictionDto>>>(url);
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch predictions");
            return null;
        }
    }

    public async Task<PredictionStatsDto?> GetPredictionStatsAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<PredictionStatsDto>>("api/admin/predictions/stats");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch prediction stats");
            return null;
        }
    }

    public async Task<IEnumerable<PromptTemplateDto>?> GetPromptTemplatesAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<PromptTemplateDto>>>("api/admin/prompts");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch prompt templates");
            return null;
        }
    }

    public async Task<PromptTemplateDto?> GetPromptTemplateByIdAsync(int id)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<ApiResponse<PromptTemplateDto>>($"api/admin/prompts/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to fetch prompt template {Id}", id);
            return null;
        }
    }

    public async Task<PromptTemplateDto?> CreatePromptTemplateAsync(CreatePromptTemplateDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/admin/prompts", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PromptTemplateDto>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to create prompt template");
            return null;
        }
    }

    public async Task<PromptTemplateDto?> UpdatePromptTemplateAsync(int id, CreatePromptTemplateDto dto)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/admin/prompts/{id}", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PromptTemplateDto>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to update prompt template {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeletePromptTemplateAsync(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/admin/prompts/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin: failed to delete prompt template {Id}", id);
            return false;
        }
    }
}
