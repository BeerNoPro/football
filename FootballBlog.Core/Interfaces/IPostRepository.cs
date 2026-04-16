using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<Post?> GetBySlugAsync(string slug);
    Task<Post?> GetDetailByIdAsync(int id);
    Task<IEnumerable<Post>> GetAllWithDetailsAsync(int page, int pageSize);
    Task<int> CountAllAsync();
    Task<IEnumerable<Post>> GetPublishedAsync(int page, int pageSize);
    Task<IEnumerable<Post>> GetByCategoryAsync(string categorySlug, int page, int pageSize);
    Task<IEnumerable<Post>> GetByTagAsync(string tagSlug, int page, int pageSize);
    Task<int> CountPublishedAsync();
    Task<int> CountByCategoryAsync(string categorySlug);
    Task<int> CountByTagAsync(string tagSlug);
}
