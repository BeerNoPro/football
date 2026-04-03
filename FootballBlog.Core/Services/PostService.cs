using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Logging;

namespace FootballBlog.Core.Services;

public class PostService : IPostService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PostService> _logger;

    public PostService(IUnitOfWork uow, ILogger<PostService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IEnumerable<PostSummaryDto>> GetPublishedAsync(int page, int pageSize)
    {
        _logger.LogDebug("Getting published posts page {Page} size {PageSize}", page, pageSize);
        var posts = await _uow.Posts.GetPublishedAsync(page, pageSize);
        return posts.Select(ToSummaryDto);
    }

    public async Task<PostDetailDto?> GetBySlugAsync(string slug)
    {
        _logger.LogDebug("Getting post by slug {Slug}", slug);
        var post = await _uow.Posts.GetBySlugAsync(slug);
        if (post is null)
        {
            _logger.LogWarning("Post not found for slug {Slug}", slug);
            return null;
        }
        return ToDetailDto(post);
    }

    public async Task<IEnumerable<PostSummaryDto>> GetByCategoryAsync(string categorySlug, int page, int pageSize)
    {
        _logger.LogDebug("Getting posts by category {CategorySlug} page {Page}", categorySlug, page);
        var posts = await _uow.Posts.GetByCategoryAsync(categorySlug, page, pageSize);
        return posts.Select(ToSummaryDto);
    }

    public async Task<IEnumerable<PostSummaryDto>> GetByTagAsync(string tagSlug, int page, int pageSize)
    {
        _logger.LogDebug("Getting posts by tag {TagSlug} page {Page}", tagSlug, page);
        var posts = await _uow.Posts.GetByTagAsync(tagSlug, page, pageSize);
        return posts.Select(ToSummaryDto);
    }

    public Task<int> CountPublishedAsync() => _uow.Posts.CountPublishedAsync();

    public Task<int> CountByCategoryAsync(string categorySlug) => _uow.Posts.CountByCategoryAsync(categorySlug);

    public Task<int> CountByTagAsync(string tagSlug) => _uow.Posts.CountByTagAsync(tagSlug);

    public async Task<PostDetailDto> CreateAsync(CreatePostDto dto)
    {
        var post = new Post
        {
            Title = dto.Title,
            Slug = dto.Slug,
            Content = dto.Content,
            Thumbnail = dto.Thumbnail,
            CategoryId = dto.CategoryId,
            AuthorId = dto.AuthorId,
            PublishedAt = dto.PublishNow ? DateTime.UtcNow : null,
        };
        await _uow.Posts.AddAsync(post);
        await _uow.CommitAsync();

        // Reload để lấy navigation properties (Category, Author)
        var created = await _uow.Posts.GetBySlugAsync(post.Slug);
        _logger.LogInformation("Post created {@Post}", new { post.Id, post.Slug });
        return ToDetailDto(created!);
    }

    public async Task<PostDetailDto?> UpdateAsync(int id, CreatePostDto dto)
    {
        var post = await _uow.Posts.GetByIdAsync(id);
        if (post is null)
        {
            _logger.LogWarning("Post not found for id {PostId}", id);
            return null;
        }

        post.Title = dto.Title;
        post.Slug = dto.Slug;
        post.Content = dto.Content;
        post.Thumbnail = dto.Thumbnail;
        post.CategoryId = dto.CategoryId;
        post.UpdatedAt = DateTime.UtcNow;
        if (dto.PublishNow && !post.IsPublished)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        await _uow.Posts.UpdateAsync(post);
        await _uow.CommitAsync();

        var updated = await _uow.Posts.GetBySlugAsync(dto.Slug);
        _logger.LogInformation("Post {PostId} updated", id);
        return updated is null ? null : ToDetailDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var post = await _uow.Posts.GetByIdAsync(id);
        if (post is null)
        {
            _logger.LogWarning("Post not found for id {PostId}", id);
            return false;
        }
        await _uow.Posts.DeleteAsync(post);
        await _uow.CommitAsync();
        _logger.LogInformation("Post {PostId} deleted", id);
        return true;
    }

    private static PostSummaryDto ToSummaryDto(Post p) => new(
        p.Id,
        p.Title,
        p.Slug,
        p.Thumbnail,
        p.Category.Name,
        p.Category.Slug,
        p.Author.UserName ?? string.Empty,
        p.PublishedAt!.Value
    );

    private static PostDetailDto ToDetailDto(Post p) => new(
        p.Id,
        p.Title,
        p.Slug,
        p.Content,
        p.Thumbnail,
        p.Category?.Name ?? string.Empty,
        p.Category?.Slug ?? string.Empty,
        p.Author?.UserName ?? string.Empty,
        p.PublishedAt ?? DateTime.UtcNow,
        p.PostTags.Select(pt => pt.Tag.Name).ToList()
    );
}
