using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
}
