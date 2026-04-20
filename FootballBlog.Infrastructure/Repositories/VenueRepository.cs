using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class VenueRepository : BaseRepository<Venue>, IVenueRepository
{
    public VenueRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Venue?> GetByExternalIdAsync(int externalId) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(v => v.ExternalId == externalId);
}
