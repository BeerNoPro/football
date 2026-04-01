using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetBySlugAsync(string slug);
}
