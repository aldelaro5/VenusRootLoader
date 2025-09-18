using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings.LogProvider;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Logging;

public sealed class ConsoleLogProvider : ILoggerProvider
{
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly ConsoleLoggerSettings _consoleLoggerSettings;

    public ConsoleLogProvider(GameExecutionContext gameExecutionContext, IOptions<ConsoleLoggerSettings> loggingSettings)
    {
        _gameExecutionContext = gameExecutionContext;
        _consoleLoggerSettings = loggingSettings.Value;
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (!_consoleLoggerSettings.Enable!.Value)
            return NullLogger.Instance;
        return new ConsoleLogger(_gameExecutionContext, categoryName);
    }

    public void Dispose() { }
}
