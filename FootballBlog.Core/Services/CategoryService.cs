using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Logging;

namespace FootballBlog.Core.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork uow, ILogger<CategoryService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        _logger.LogDebug("Getting all categories");
        var categories = await _uow.Categories.GetAllAsync();
        return categories.Select(ToDto);
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        _logger.LogDebug("Getting category by slug {Slug}", slug);
        var category = await _uow.Categories.GetBySlugAsync(slug);
        if (category is null)
        {
            _logger.LogWarning("Category not found for slug {Slug}", slug);
            return null;
        }
        return ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(string name, string slug)
    {
        var category = new Category { Name = name, Slug = slug };
        await _uow.Categories.AddAsync(category);
        await _uow.CommitAsync();
        _logger.LogInformation("Category created {@Category}", new { category.Id, category.Slug });
        return ToDto(category);
    }

    private static CategoryDto ToDto(Category c) => new(c.Id, c.Name, c.Slug);
}
