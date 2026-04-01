using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<Category?> GetBySlugAsync(string slug) =>
        await _dbSet.FirstOrDefaultAsync(c => c.Slug == slug);
}
