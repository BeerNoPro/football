using FootballBlog.Core.Interfaces;
using FootballBlog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IPostRepository Posts { get; }
    public ICategoryRepository Categories { get; }
    public ITagRepository Tags { get; }
    public ILiveMatchRepository LiveMatches { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Posts = new PostRepository(context);
        Categories = new CategoryRepository(context);
        Tags = new TagRepository(context);
        LiveMatches = new LiveMatchRepository(context);
    }

    public Task<int> CommitAsync() => _context.SaveChangesAsync();

    public Task RollbackAsync()
    {
        // Detach tất cả tracked entities — huỷ mọi thay đổi chưa commit
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            entry.State = EntityState.Detached;
        }

        return Task.CompletedTask;
    }

    public void Dispose() => _context.Dispose();
}
