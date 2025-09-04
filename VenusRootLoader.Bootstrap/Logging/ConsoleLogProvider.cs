using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Services;

namespace VenusRootLoader.Bootstrap.Logging;

public class ConsoleLogProvider : ILoggerProvider
{
    public void Dispose() { }

    private readonly GameExecutionContext _gameExecutionContext;

    public ConsoleLogProvider(GameExecutionContext gameExecutionContext)
    {
        _gameExecutionContext = gameExecutionContext;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new ConsoleLogger(_gameExecutionContext, categoryName);
    }
}
