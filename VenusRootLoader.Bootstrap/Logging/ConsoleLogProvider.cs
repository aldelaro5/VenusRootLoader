using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Bootstrap;

public class ConsoleLogProvider : ILoggerProvider
{
    public void Dispose() { }

    public ILogger CreateLogger(string categoryName)
    {
        var categoryWithColorInfo = ColoredLoggerCategory.Decode(categoryName);
        return new ConsoleLogger(categoryWithColorInfo.category, categoryWithColorInfo.color);
    }
}
