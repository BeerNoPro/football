using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

/// <summary>Typed HTTP client để gọi Tag endpoints trên FootballBlog.API.</summary>
public interface ITagApiClient
{
    Task<IEnumerable<TagDto>> GetAllAsync();
    Task<TagDto?> GetBySlugAsync(string slug);
    Task<PagedResult<PostSummaryDto>?> GetPostsAsync(string slug, int page = 1, int pageSize = 10);
}
