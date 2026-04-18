namespace FootballBlog.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IPostRepository Posts { get; }
    ICategoryRepository Categories { get; }
    ITagRepository Tags { get; }
    ILiveMatchRepository LiveMatches { get; }
    IMatchRepository Matches { get; }
    IMatchPredictionRepository MatchPredictions { get; }
    ICountryRepository Countries { get; }
    ILeagueRepository Leagues { get; }
    ITeamRepository Teams { get; }
    IMatchContextRepository MatchContexts { get; }
    IPromptTemplateRepository PromptTemplates { get; }

    /// <summary>Commit tất cả thay đổi trong 1 transaction.</summary>
    Task<int> CommitAsync();

    /// <summary>Huỷ tất cả thay đổi chưa commit (detach toàn bộ tracked entities).</summary>
    Task RollbackAsync();
}
