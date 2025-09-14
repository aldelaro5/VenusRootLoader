using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Bootstrap.Logging;

public class FileLogger : ILogger
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
    private readonly StreamWriter _logWriter;

    public FileLogger(string categoryName, StreamWriter logWriter)
    {
        var simplifiedCategoryName = categoryName;
        var lastDotIndex = categoryName.LastIndexOf('.');
        if (lastDotIndex > -1)
            simplifiedCategoryName = categoryName[(lastDotIndex + 1) ..];

        _categoryName = simplifiedCategoryName;
        _logWriter = logWriter;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var time = DateTime.Now.ToString("HH:mm:ss.fff");

        string message = formatter(state, exception);
        if (exception is not null)
            message += $" {exception}";

        _logWriter.WriteLine($"[{time}] " +
                             $"[{_logLevelInfos[logLevel]}] " +
                             $"[{_categoryName}] " +
                             $"{message}");
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}