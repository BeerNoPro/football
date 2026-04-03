using FootballBlog.API.Common;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
    {
        var categories = await categoryService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(categories));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetBySlug(string slug)
    {
        var category = await categoryService.GetBySlugAsync(slug);
        if (category is null)
        {
            logger.LogWarning("Category not found for slug {Slug}", slug);
            return NotFound(ApiResponse<CategoryDto>.Fail($"Category '{slug}' not found"));
        }
        return Ok(ApiResponse<CategoryDto>.Ok(category));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        var category = await categoryService.CreateAsync(dto.Name, dto.Slug);
        logger.LogInformation("Category created {Slug}", category.Slug);
        return CreatedAtAction(nameof(GetBySlug), new { slug = category.Slug }, ApiResponse<CategoryDto>.Ok(category));
    }
}

public record CreateCategoryDto(string Name, string Slug);
