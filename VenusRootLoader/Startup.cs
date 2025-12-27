using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using VenusRootLoader.BudLoading;
using VenusRootLoader.Config;
using VenusRootLoader.Extensions;
using VenusRootLoader.Logging;
using VenusRootLoader.Modding;
using VenusRootLoader.Patching;
using VenusRootLoader.Patching.TextAssetData;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;

namespace VenusRootLoader;

internal static class Startup
{
    internal static IServiceProvider BuildServiceProvider(
        string basePath,
        GameExecutionContext gameExecutionContext,
        BootstrapFunctions bootstrapFunctions)
    {
        IServiceCollection services = new ServiceCollection();
        IConfigurationManager configurationManager = new ConfigurationManager();

        FileSystem fileSystem = new();
        string configPath = fileSystem.Path.Combine(basePath, "Config");
        configurationManager.AddJsonFile(fileSystem.Path.Combine(configPath, "config.jsonc"));

        services.AddSingleton(gameExecutionContext);
        services.AddSingleton(bootstrapFunctions);
        services.AddSingleton(
            new BudLoaderContext
            {
                BudsPath = fileSystem.Path.Combine(basePath, "Buds"),
                ConfigPath = configPath,
                LoaderPath = fileSystem.Path.Combine(gameExecutionContext.GameDir, nameof(VenusRootLoader)),
            });

        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configurationManager.GetRequiredSection("Logging"));
            builder.Services.AddSingleton<ILoggerProvider, RelayLoggerProvider>();
        });

        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IAppDomainEvents, AppDomainEvents>();
        services.AddSingleton<AppDomainEventsHandler>();
        services.AddSingleton<HarmonyLogger>();
        services.AddSingleton<IHarmonyTypePatcher, HarmonyTypePatcher>();

        services.AddSingleton<EnumPatcher>();
        services.AddSingleton<ResourcesPatcher>();
        services.AddSingleton<RootTextAssetPatcher>();
        services.AddBoundTextAssetPatcher<ItemData>("Data/ItemData");
        services.AddBoundLocalizedTextAssetPatcher<ItemLanguageData>("Items");
        services.AddSingleton<ItemAndMedalSpritePatcher>();

        services.AddSingleton<ItemRegistry>();

        services.AddSingleton<GlobalMonoBehaviourExecution>();

        services.AddSingleton<IBudConfigManager, BudConfigManager>();
        services.AddSingleton<IVenusFactory, VenusFactory>();
        services.AddSingleton<IBudsDiscoverer, BudsDiscoverer>();
        services.AddSingleton<IBudsValidator, BudsValidator>();
        services.AddSingleton<IBudsDependencySorter, BudsDependencySorter>();
        services.AddSingleton<IBudsLoadOrderEnumerator, BudsLoadOrderEnumerator>();
        services.AddSingleton<IAssemblyLoader, AssemblyLoader>();
        services.AddSingleton<BudLoader>();

        return services.BuildServiceProvider();
    }
}