using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Logging;

namespace VenusRootLoader.Bootstrap.Extensions;

public static class LoggingBuilderExtensions
{
    public static void AddConsoleLoggingProvider(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, ConsoleLogProvider>();
    }

    public static void AddFileLoggingProvider(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, DiskFileLoggerProvider>();
    }
}