using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FootballBlog.Infrastructure.Data;

public class QueryLoggingInterceptor(ILogger<QueryLoggingInterceptor> logger) : DbCommandInterceptor
{
    private const int SlowQueryThresholdMs = 500;

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData data, DbDataReader result)
    {
        Log(command.CommandText, data.Duration);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData data, DbDataReader result, CancellationToken cancellationToken = default)
    {
        Log(command.CommandText, data.Duration);
        return new ValueTask<DbDataReader>(result);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData data, object? result)
    {
        Log(command.CommandText, data.Duration);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData data, object? result, CancellationToken cancellationToken = default)
    {
        Log(command.CommandText, data.Duration);
        return new ValueTask<object?>(result);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData data, int result)
    {
        Log(command.CommandText, data.Duration);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData data, int result, CancellationToken cancellationToken = default)
    {
        Log(command.CommandText, data.Duration);
        return new ValueTask<int>(result);
    }

    private void Log(string sql, TimeSpan duration)
    {
        long ms = (long)duration.TotalMilliseconds;

        if (ms >= SlowQueryThresholdMs)
        {
            logger.LogWarning("[SLOW SQL] {ElapsedMs}ms\n{Sql}", ms, sql);
        }
        else
        {
            logger.LogDebug("[SQL] {ElapsedMs}ms\n{Sql}", ms, sql);
        }
    }
}
