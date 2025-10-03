using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Bootstrap.Logging;

public class DiskFileLogger : ILogger
{
    private readonly Dictionary<LogLevel, string> _logLevelInfos = new()
    {
        { LogLevel.Critical, "!" },
        { LogLevel.Error, "E" },
        { LogLevel.Warning, "W" },
        { LogLevel.Information, "I" },
        { LogLevel.Debug, "D" },
        { LogLevel.Trace, "T" }
    };

    private readonly string _categoryName;
    private readonly TimeProvider _timeProvider;
    private readonly StreamWriter _logWriter;

    public DiskFileLogger(string categoryName, StreamWriter logWriter, TimeProvider timeProvider)
    {
        var simplifiedCategoryName = categoryName;
        var lastDotIndex = categoryName.LastIndexOf('.');
        if (lastDotIndex > -1)
            simplifiedCategoryName = categoryName[(lastDotIndex + 1)..];

        _categoryName = simplifiedCategoryName;
        _logWriter = logWriter;
        _timeProvider = timeProvider;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var time = _timeProvider.GetLocalNow().ToString("HH:mm:ss.fff");

        string message = formatter(state, exception);
        if (exception is not null)
            message += $" {exception}";

        _logWriter.WriteLine(
            $"[{time}] " +
            $"[{_logLevelInfos[logLevel]}] " +
            $"[{_categoryName}] " +
            $"{message}");
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}