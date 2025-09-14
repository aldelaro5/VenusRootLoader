using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Logging;

public class ConsoleLogProvider : ILoggerProvider
{
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly LoggingSettings _loggingSettings;

    public ConsoleLogProvider(GameExecutionContext gameExecutionContext, IOptions<LoggingSettings> loggingSettings)
    {
        _gameExecutionContext = gameExecutionContext;
        _loggingSettings = loggingSettings.Value;
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (!_loggingSettings.ShowConsole!.Value)
            return NullLogger.Instance;
        return new ConsoleLogger(_gameExecutionContext, categoryName);
    }

    public void Dispose() { }
}
