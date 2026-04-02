namespace FootballBlog.Web.ApiClients;

/// <summary>Typed HTTP client để gọi Post endpoints trên FootballBlog.API.</summary>
/// <remarks>Methods thực tế sẽ được thêm ở Phase 2 khi có DTOs.</remarks>
public interface IPostApiClient
{
    // Phase 2: Task<IEnumerable<PostSummaryDto>> GetPublishedAsync(int page, int pageSize);
    // Phase 2: Task<PostDetailDto?> GetBySlugAsync(string slug);
    // Phase 2: Task<IEnumerable<PostSummaryDto>> GetByCategoryAsync(string categorySlug, int page, int pageSize);
}
