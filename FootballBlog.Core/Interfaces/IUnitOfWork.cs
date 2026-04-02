namespace FootballBlog.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IPostRepository Posts { get; }
    ICategoryRepository Categories { get; }
    ITagRepository Tags { get; }
    ILiveMatchRepository LiveMatches { get; }

    /// <summary>Commit tất cả thay đổi trong 1 transaction.</summary>
    Task<int> CommitAsync();

    /// <summary>Huỷ tất cả thay đổi chưa commit (detach toàn bộ tracked entities).</summary>
    Task RollbackAsync();
}
