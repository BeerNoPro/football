using FootballBlog.Core.DTOs;

namespace FootballBlog.Core.Interfaces.Services;

public interface IPostService
{
    Task<IEnumerable<PostSummaryDto>> GetPublishedAsync(int page, int pageSize);
    Task<PostDetailDto?> GetBySlugAsync(string slug);
    Task<PostDetailDto?> GetByIdAsync(int id);
    Task<IEnumerable<PostSummaryDto>> GetAllAsync(int page, int pageSize);
    Task<int> CountAllAsync();
    Task<IEnumerable<PostSummaryDto>> GetByCategoryAsync(string categorySlug, int page, int pageSize);
    Task<IEnumerable<PostSummaryDto>> GetByTagAsync(string tagSlug, int page, int pageSize);
    Task<int> CountPublishedAsync();
    Task<int> CountByCategoryAsync(string categorySlug);
    Task<int> CountByTagAsync(string tagSlug);
    Task<PostDetailDto> CreateAsync(CreatePostDto dto);
    Task<PostDetailDto?> UpdateAsync(int id, CreatePostDto dto);
    Task<bool> DeleteAsync(int id);
}
