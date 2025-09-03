using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Logging;

namespace VenusRootLoader.Bootstrap.Extensions;

public static class LoggingExtensions
{
    public static void AddConsoleLoggingProvider(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, ConsoleLogProvider>();
    }

    public static ILogger CreateLogger(this ILoggerFactory factory, string categoryName, Color categoryColor)
    {
        return factory.CreateLogger(ColoredLoggerCategory.Encode(categoryName, categoryColor));
    }
}