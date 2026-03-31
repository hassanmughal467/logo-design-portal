using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Infrastructure.Persistence;

/// <summary>Logs database commands that exceed a duration threshold (SLO: 100 ms).</summary>
public sealed class SlowQueryLoggingInterceptor : DbCommandInterceptor
{
    private readonly ILogger<SlowQueryLoggingInterceptor> _logger;

    public SlowQueryLoggingInterceptor(ILogger<SlowQueryLoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(eventData, command);
        return ValueTask.FromResult(result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(eventData, command);
        return ValueTask.FromResult(result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(eventData, command);
        return ValueTask.FromResult(result);
    }

    private void LogIfSlow(CommandExecutedEventData eventData, DbCommand command)
    {
        if (eventData.Duration.TotalMilliseconds < 100)
            return;

        var text = command.CommandText;
        if (text.Length > 500)
            text = text[..500] + "…";

        _logger.LogWarning(
            "Slow database command: {DurationMs:F0} ms (SLO target 100 ms). {CommandText}",
            eventData.Duration.TotalMilliseconds,
            text);
    }
}
