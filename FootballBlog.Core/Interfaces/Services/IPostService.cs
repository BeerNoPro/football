using FootballBlog.Core.DTOs;

namespace FootballBlog.Core.Interfaces.Services;

public interface IPostService
{
    Task<IEnumerable<PostSummaryDto>> GetPublishedAsync(int page, int pageSize);
    Task<PostDetailDto?> GetBySlugAsync(string slug);
    Task<IEnumerable<PostSummaryDto>> GetByCategoryAsync(string categorySlug, int page, int pageSize);
    Task<IEnumerable<PostSummaryDto>> GetByTagAsync(string tagSlug, int page, int pageSize);
    Task<int> CountPublishedAsync();
    Task<PostDetailDto> CreateAsync(CreatePostDto dto);
    Task<PostDetailDto?> UpdateAsync(int id, CreatePostDto dto);
    Task<bool> DeleteAsync(int id);
}
