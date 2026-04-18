using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IPromptTemplateRepository : IRepository<PromptTemplate>
{
    Task<PromptTemplate?> GetActiveByProviderAsync(string provider);
}
