using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface ICountryRepository : IRepository<Country>
{
    Task<Country?> GetByCodeAsync(string code);
}
