using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

public interface IAdminApiClient
{
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
}
