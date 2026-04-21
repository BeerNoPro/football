using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Data;

public static class QueryableExtensions
{
    public static IQueryable<T> TagWithCaller<T>(
        this IQueryable<T> query,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "") =>
        query.TagWith($"{Path.GetFileNameWithoutExtension(file)}.{member}");
}
