using FootballBlog.Core.DTOs;

namespace FootballBlog.Web.ApiClients;

/// <summary>Typed HTTP client để gọi Category endpoints trên FootballBlog.API.</summary>
public interface ICategoryApiClient
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetBySlugAsync(string slug);
}
