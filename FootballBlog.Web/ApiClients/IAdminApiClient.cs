using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public interface IAdminApiClient
{
    // Posts
    Task<PagedResult<PostSummaryDto>?> GetAllPostsAsync(int page = 1, int pageSize = 20);
    Task<bool> DeletePostAsync(int id);

    // Categories
    Task<IEnumerable<CategoryDto>?> GetCategoriesAsync();
    Task<CategoryDto?> CreateCategoryAsync(string name, string slug);
    Task<bool> DeleteCategoryAsync(int id);

    // Tags
    Task<IEnumerable<TagDto>?> GetTagsAsync();
    Task<bool> DeleteTagAsync(int id);
}
