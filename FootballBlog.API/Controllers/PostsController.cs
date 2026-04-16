using FootballBlog.API.Common;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController(IPostService postService, ILogger<PostsController> logger, IOutputCacheStore cacheStore) : ControllerBase
{
    [HttpGet]
    [OutputCache(PolicyName = "BlogPages")]
    public async Task<ActionResult<ApiResponse<PagedResult<PostSummaryDto>>>> GetPublished(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var items = await postService.GetPublishedAsync(page, pageSize);
        var total = await postService.CountPublishedAsync();
        return Ok(ApiResponse<PagedResult<PostSummaryDto>>.Ok(new PagedResult<PostSummaryDto>(items, page, pageSize, total)));
    }

    [HttpGet("{slug}")]
    [OutputCache(PolicyName = "BlogPages")]
    public async Task<ActionResult<ApiResponse<PostDetailDto>>> GetBySlug(string slug)
    {
        var post = await postService.GetBySlugAsync(slug);
        if (post is null)
        {
            logger.LogWarning("Post not found for slug {Slug}", slug);
            return NotFound(ApiResponse<PostDetailDto>.Fail($"Post '{slug}' not found"));
        }
        return Ok(ApiResponse<PostDetailDto>.Ok(post));
    }

    [HttpGet("by-category/{categorySlug}")]
    [OutputCache(PolicyName = "BlogPages")]
    public async Task<ActionResult<ApiResponse<PagedResult<PostSummaryDto>>>> GetByCategory(
        string categorySlug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var items = await postService.GetByCategoryAsync(categorySlug, page, pageSize);
        var total = await postService.CountByCategoryAsync(categorySlug);
        return Ok(ApiResponse<PagedResult<PostSummaryDto>>.Ok(new PagedResult<PostSummaryDto>(items, page, pageSize, total)));
    }

    [HttpGet("by-tag/{tagSlug}")]
    [OutputCache(PolicyName = "BlogPages")]
    public async Task<ActionResult<ApiResponse<PagedResult<PostSummaryDto>>>> GetByTag(
        string tagSlug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var items = await postService.GetByTagAsync(tagSlug, page, pageSize);
        var total = await postService.CountByTagAsync(tagSlug);
        return Ok(ApiResponse<PagedResult<PostSummaryDto>>.Ok(new PagedResult<PostSummaryDto>(items, page, pageSize, total)));
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResult<PostSummaryDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var items = await postService.GetAllAsync(page, pageSize);
        var total = await postService.CountAllAsync();
        return Ok(ApiResponse<PagedResult<PostSummaryDto>>.Ok(new PagedResult<PostSummaryDto>(items, page, pageSize, total)));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PostDetailDto>>> GetById(int id)
    {
        var post = await postService.GetByIdAsync(id);
        if (post is null)
        {
            logger.LogWarning("Post not found for id {PostId}", id);
            return NotFound(ApiResponse<PostDetailDto>.Fail($"Post {id} not found"));
        }
        return Ok(ApiResponse<PostDetailDto>.Ok(post));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PostDetailDto>>> Create([FromBody] CreatePostDto dto)
    {
        var post = await postService.CreateAsync(dto);
        await cacheStore.EvictByTagAsync("posts", default);
        logger.LogInformation("Post created {Slug}", post.Slug);
        return CreatedAtAction(nameof(GetBySlug), new { slug = post.Slug }, ApiResponse<PostDetailDto>.Ok(post));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PostDetailDto>>> Update(int id, [FromBody] CreatePostDto dto)
    {
        var post = await postService.UpdateAsync(id, dto);
        if (post is null)
        {
            return NotFound(ApiResponse<PostDetailDto>.Fail($"Post {id} not found"));
        }

        await cacheStore.EvictByTagAsync("posts", default);
        return Ok(ApiResponse<PostDetailDto>.Ok(post));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var deleted = await postService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<bool>.Fail($"Post {id} not found"));
        }

        await cacheStore.EvictByTagAsync("posts", default);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
