using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Services;

namespace VenusRootLoader.Bootstrap.Extensions;

public static class LoggingExtensions
{
    public static void AddConsoleLoggingProvider(this ILoggingBuilder builder, GameExecutionContext context)
    {
        builder.Services.AddSingleton<GameExecutionContext>(_ => context);
        builder.Services.AddSingleton<ILoggerProvider, ConsoleLogProvider>();
    }

    public static ILogger CreateLogger(this ILoggerFactory factory, string categoryName, Color categoryColor)
    {
        return factory.CreateLogger(ColoredLoggerCategory.Encode(categoryName, categoryColor));
    }
}