using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace VenusRootLoader.Preloader;

internal static class Startup
{
    internal static IHost BuildHost(GameExecutionContext gameExecutionContext, BootstrapFunctions bootstrapFunctions)
    {
        var builder = Host.CreateEmptyApplicationBuilder(
            new()
            {
                DisableDefaults = true,
                ApplicationName = "VenusRootLoader",
                Args = [],
                EnvironmentName = "Development",
                ContentRootPath = gameExecutionContext.GameDir,
                Configuration = new()
            });

        var fileSystem = new FileSystem();

        builder.Configuration.AddJsonFile(
            fileSystem.Path.Combine(builder.Environment.ContentRootPath, "Config", "config.jsonc"));
        builder.Services.AddSingleton(gameExecutionContext);
        builder.Services.AddSingleton(bootstrapFunctions);
        builder.Logging.AddConfiguration(builder.Configuration.GetRequiredSection("Logging"));
        builder.Logging.Services.AddSingleton<ILoggerProvider, RelayLoggerProvider>();
        builder.Services.AddHostedService<HarmonyLogger>();
        builder.Services.AddHostedService<GameLoadEntrypointInitializer>();

        return builder.Build();
    }
}