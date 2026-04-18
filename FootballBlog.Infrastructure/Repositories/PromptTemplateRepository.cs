using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Repositories;

public class PromptTemplateRepository(ApplicationDbContext dbContext)
    : BaseRepository<PromptTemplate>(dbContext), IPromptTemplateRepository
{
    public async Task<PromptTemplate?> GetActiveByProviderAsync(string provider) =>
        await _dbSet
            .AsNoTracking()
            .Where(t => t.Provider == provider && t.IsActive)
            .OrderByDescending(t => t.UpdatedAt)
            .FirstOrDefaultAsync();
}
