using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

/// <summary>Typed HTTP client để gọi Post endpoints trên FootballBlog.API.</summary>
public interface IPostApiClient
{
    Task<PagedResult<PostSummaryDto>?> GetPublishedAsync(int page = 1, int pageSize = 10);
    Task<PostDetailDto?> GetBySlugAsync(string slug);
    Task<PagedResult<PostSummaryDto>?> GetByCategoryAsync(string categorySlug, int page = 1, int pageSize = 10);
    Task<PagedResult<PostSummaryDto>?> GetByTagAsync(string tagSlug, int page = 1, int pageSize = 10);
}
