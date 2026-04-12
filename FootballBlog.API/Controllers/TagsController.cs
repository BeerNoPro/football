using FootballBlog.API.Common;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController(IUnitOfWork uow, IPostService postService, ILogger<TagsController> logger) : ControllerBase
{
    [HttpGet]
    [OutputCache(PolicyName = "BlogPages")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TagDto>>>> GetAll()
    {
        var tags = await uow.Tags.GetAllAsync();
        var dtos = tags.Select(t => new TagDto(t.Id, t.Name, t.Slug));
        return Ok(ApiResponse<IEnumerable<TagDto>>.Ok(dtos));
    }

    [HttpGet("{slug}")]
    [OutputCache(PolicyName = "BlogPages")]
    public async Task<ActionResult<ApiResponse<TagDto>>> GetBySlug(string slug)
    {
        var tag = await uow.Tags.GetBySlugAsync(slug);
        if (tag is null)
        {
            logger.LogWarning("Tag not found for slug {Slug}", slug);
            return NotFound(ApiResponse<TagDto>.Fail($"Tag '{slug}' not found"));
        }
        return Ok(ApiResponse<TagDto>.Ok(new TagDto(tag.Id, tag.Name, tag.Slug)));
    }

    [HttpGet("{slug}/posts")]
    [OutputCache(PolicyName = "BlogPages")]
    public async Task<ActionResult<ApiResponse<PagedResult<PostSummaryDto>>>> GetPosts(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var items = await postService.GetByTagAsync(slug, page, pageSize);
        var total = await postService.CountByTagAsync(slug);
        return Ok(ApiResponse<PagedResult<PostSummaryDto>>.Ok(new PagedResult<PostSummaryDto>(items, page, pageSize, total)));
    }
}
