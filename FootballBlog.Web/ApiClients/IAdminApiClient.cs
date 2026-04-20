using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public interface IAdminApiClient
{
    // API Keys
    Task<IEnumerable<ApiKeyDto>?> GetApiKeysAsync();
    Task<ApiKeyDto?> CreateApiKeyAsync(CreateApiKeyDto dto);
    Task<ApiKeyDto?> ToggleApiKeyAsync(int id);
    Task<bool> DeleteApiKeyAsync(int id);

    // Posts
    Task<PagedResult<PostSummaryDto>?> GetAllPostsAsync(int page = 1, int pageSize = 20);
    Task<PostDetailDto?> GetPostByIdAsync(int id);
    Task<PostDetailDto?> CreatePostAsync(CreatePostDto dto);
    Task<PostDetailDto?> UpdatePostAsync(int id, CreatePostDto dto);
    Task<bool> DeletePostAsync(int id);
    Task<string?> UploadImageAsync(Stream stream, string fileName, string contentType);

    // Categories
    Task<IEnumerable<CategoryDto>?> GetCategoriesAsync();
    Task<CategoryDto?> CreateCategoryAsync(string name, string slug);
    Task<bool> DeleteCategoryAsync(int id);

    // Tags
    Task<IEnumerable<TagDto>?> GetTagsAsync();
    Task<bool> DeleteTagAsync(int id);

    // Matches
    Task<PagedResult<MatchSummaryDto>?> GetMatchesAsync(int page = 1, int pageSize = 20, string? status = null, bool? hasPrediction = null, string? search = null);
    Task<MatchStatsDto?> GetMatchStatsAsync();
    Task<bool> TriggerFetchMatchesAsync();
    Task<bool> TriggerPredictAllAsync();
    Task<bool> TriggerSeedLeagueDataAsync();

    // Predictions
    Task<PagedResult<MatchPredictionDto>?> GetPredictionsAsync(int page = 1, int pageSize = 20, bool? isPublished = null);
    Task<PredictionStatsDto?> GetPredictionStatsAsync();

    // Prompt Templates
    Task<IEnumerable<PromptTemplateDto>?> GetPromptTemplatesAsync();
    Task<PromptTemplateDto?> GetPromptTemplateByIdAsync(int id);
    Task<PromptTemplateDto?> CreatePromptTemplateAsync(CreatePromptTemplateDto dto);
    Task<PromptTemplateDto?> UpdatePromptTemplateAsync(int id, CreatePromptTemplateDto dto);
    Task<bool> DeletePromptTemplateAsync(int id);
}
