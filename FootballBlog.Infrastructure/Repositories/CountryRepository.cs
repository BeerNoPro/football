using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class CountryRepository : BaseRepository<Country>, ICountryRepository
{
    public CountryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Country?> GetByCodeAsync(string code) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code);
}
