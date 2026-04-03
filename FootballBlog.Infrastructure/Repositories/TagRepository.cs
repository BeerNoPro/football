using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<Tag?> GetBySlugAsync(string slug) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug);
}
