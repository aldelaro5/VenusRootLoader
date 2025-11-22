using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Logging;

internal class RelayLogger : ILogger
{
    private readonly BootstrapFunctions _bootstrapFunctions;
    private readonly string _categoryName;

    public RelayLogger(BootstrapFunctions bootstrapFunctions, string categoryName)
    {
        _bootstrapFunctions = bootstrapFunctions;
        _categoryName = categoryName;
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

        string message = formatter(state, exception);
        if (exception is not null)
            message += $" {exception}";

        _bootstrapFunctions.BootstrapLog(message, _categoryName, logLevel);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}