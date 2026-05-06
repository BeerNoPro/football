using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FootballBlog.Infrastructure.Services;

public class ApiUsageTracker(IDbContextFactory<ApplicationDbContext> dbFactory, IConfiguration configuration) : IApiUsageTracker
{
    public async Task<bool> CanCallAsync(string service)
    {
        int limit = GetLimit(service);
        if (limit == 0)
        {
            return true;
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync();
        ApiUsageDaily? record = await db.ApiUsageDaily
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Date == today && r.Service == service);

        return record is null || record.DailyCount < limit;
    }

    public async Task IncrementAsync(string service)
    {
        int limit = GetLimit(service);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync();

        // Atomic UPDATE — tránh race condition khi concurrent calls
        int updated = await db.ApiUsageDaily
            .Where(r => r.Date == today && r.Service == service)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.DailyCount, r => r.DailyCount + 1)
                .SetProperty(r => r.UpdatedAt, DateTime.UtcNow));

        if (updated == 0)
        {
            // Lần đầu trong ngày — INSERT, unique index bắt conflict nếu concurrent
            try
            {
                db.ApiUsageDaily.Add(new ApiUsageDaily
                {
                    Date = today,
                    Service = service,
                    DailyCount = 1,
                    DailyLimit = limit,
                    UpdatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Concurrent INSERT đã thắng — retry UPDATE
                await db.ApiUsageDaily
                    .Where(r => r.Date == today && r.Service == service)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.DailyCount, r => r.DailyCount + 1)
                        .SetProperty(r => r.UpdatedAt, DateTime.UtcNow));
            }
        }
    }

    public async Task<(int Count, int Limit)> GetTodayAsync(string service)
    {
        int limit = GetLimit(service);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync();
        ApiUsageDaily? record = await db.ApiUsageDaily
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Date == today && r.Service == service);

        return (record?.DailyCount ?? 0, limit);
    }

    private int GetLimit(string service) =>
        configuration.GetValue<int>($"ApiLimits:{service}");
}
