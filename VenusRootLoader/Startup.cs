using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using VenusRootLoader.Logging;
using VenusRootLoader.BudLoading;

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
            new BudLoaderContext
            {
                BudsPath = fileSystem.Path.Combine(builder.Environment.ContentRootPath, "Buds"),
                ConfigPath = configPath,
                LoaderPath = fileSystem.Path.Combine(builder.Environment.ContentRootPath, nameof(VenusRootLoader)),
            });

        builder.Logging.AddConfiguration(builder.Configuration.GetRequiredSection("Logging"));
        builder.Logging.Services.AddSingleton<ILoggerProvider, RelayLoggerProvider>();

        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IAppDomainEvents, AppDomainEvents>();
        builder.Services.AddHostedService<AppDomainEventsHandler>();
        builder.Services.AddHostedService<HarmonyLogger>();

        builder.Services.AddSingleton<IBudsDiscoverer, BudsDiscoverer>();
        builder.Services.AddSingleton<IBudsValidator, BudsValidator>();
        builder.Services.AddSingleton<IBudsDependencySorter, BudsDependencySorter>();
        builder.Services.AddSingleton<IBudsLoadOrderEnumerator, BudsLoadOrderEnumerator>();
        builder.Services.AddSingleton<IAssemblyLoader, AssemblyLoader>();
        builder.Services.AddHostedService<BudLoader>();

        return builder.Build();
    }
}