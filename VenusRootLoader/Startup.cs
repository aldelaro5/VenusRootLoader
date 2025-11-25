using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using VenusRootLoader.Logging;
using VenusRootLoader.ModLoading;

namespace VenusRootLoader;

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

        string configPath = fileSystem.Path.Combine(builder.Environment.ContentRootPath, "Config");
        builder.Configuration.AddJsonFile(fileSystem.Path.Combine(configPath, "config.jsonc"));

        builder.Services.AddSingleton(gameExecutionContext);
        builder.Services.AddSingleton(bootstrapFunctions);
        builder.Services.AddSingleton(
            new ModLoaderContext
            {
                ModsPath = fileSystem.Path.Combine(builder.Environment.ContentRootPath, "Mods"),
                ConfigPath = configPath,
                LoaderPath = fileSystem.Path.Combine(builder.Environment.ContentRootPath, nameof(VenusRootLoader)),
            });

        builder.Logging.AddConfiguration(builder.Configuration.GetRequiredSection("Logging"));
        builder.Logging.Services.AddSingleton<ILoggerProvider, RelayLoggerProvider>();

        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddHostedService<AppDomainEventsHandler>();
        builder.Services.AddHostedService<HarmonyLogger>();

        builder.Services.AddSingleton<IModsDiscovery, ModsDiscovery>();
        builder.Services.AddSingleton<IModsSorter, ModsSorter>();
        builder.Services.AddSingleton<IModsEnumerator, ModsEnumerator>();
        builder.Services.AddHostedService<ModLoader>();

        return builder.Build();
    }
}