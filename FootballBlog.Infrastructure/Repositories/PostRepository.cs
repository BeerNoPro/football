using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class PostRepository : BaseRepository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<Post?> GetBySlugAsync(string slug) =>
        await _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.PublishedAt != null);

    public async Task<IEnumerable<Post>> GetPublishedAsync(int page, int pageSize) =>
        await _dbSet
            .AsNoTracking()
            .Where(p => p.PublishedAt != null)
            .Include(p => p.Category)
            .Include(p => p.Author)
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<Post>> GetByCategoryAsync(string categorySlug, int page, int pageSize) =>
        await _dbSet
            .AsNoTracking()
            .Where(p => p.PublishedAt != null && p.Category.Slug == categorySlug)
            .Include(p => p.Category)
            .Include(p => p.Author)
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<Post>> GetByTagAsync(string tagSlug, int page, int pageSize) =>
        await _dbSet
            .AsNoTracking()
            .Where(p => p.PublishedAt != null && p.PostTags.Any(pt => pt.Tag.Slug == tagSlug))
            .Include(p => p.Category)
            .Include(p => p.Author)
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> CountPublishedAsync() =>
        await _dbSet.CountAsync(p => p.PublishedAt != null);

    public async Task<int> CountByCategoryAsync(string categorySlug) =>
        await _dbSet.CountAsync(p => p.PublishedAt != null && p.Category.Slug == categorySlug);

    public async Task<int> CountByTagAsync(string tagSlug) =>
        await _dbSet.CountAsync(p => p.PublishedAt != null && p.PostTags.Any(pt => pt.Tag.Slug == tagSlug));
}
