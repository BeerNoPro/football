using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IVenueRepository : IRepository<Venue>
{
    Task<Venue?> GetByExternalIdAsync(int externalId);
}
