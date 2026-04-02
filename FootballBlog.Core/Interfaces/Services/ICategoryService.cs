using FootballBlog.Core.DTOs;

namespace FootballBlog.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetBySlugAsync(string slug);
    Task<CategoryDto> CreateAsync(string name, string slug);
}
