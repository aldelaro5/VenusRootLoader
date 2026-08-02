using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using VenusRootLoader.Bootstrap.Extensions;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Mono;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Settings.LogProvider;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Unity;
using VenusRootLoader.Bootstrap.Unity.GlobalManagers;
using ValidateLoggingSettings = VenusRootLoader.Bootstrap.Settings.ValidateLoggingSettings;

namespace VenusRootLoader.Bootstrap;

internal static class Startup
{
    // To enforce consistencies between environment variables and command line arguments, we enforce the following:
    // - All environment variables are all uppercase in snake case and prefixed by VRL_ (the mappings below omits the prefix)
    // - All command line arguments are their environment variables counterpart, but in all lowercase, in snake case and prefixed with "--"
    // - All command line arguments are specified in pair: the first one is the key and the next one is assumed to be the value
    // - The only environment variable not in the mapping below is VRL_BASE_DIRECTORY because it can't come from a
    //   config file and needs to be loaded early
    private static readonly Dictionary<string, string> EnvironmentVariablesConfigMapping = new()
    {
        ["SKIP_UNITY_SPLASHSCREEN"] = $"{nameof(GlobalSettings.SkipUnitySplashScreen)}",
        ["GLOBAL_DISABLE"] = $"{nameof(GlobalSettings.DisableVrl)}",
        ["ENABLE_CONSOLE_LOGS"] =
            $"{nameof(LoggingSettings)}:{nameof(ConsoleLoggerSettings)}:{nameof(ConsoleLoggerSettings.Enable)}",
        ["CONSOLE_COLORS"] =
            $"{nameof(LoggingSettings)}:{nameof(ConsoleLoggerSettings)}:{nameof(ConsoleLoggerSettings.LogWithColors)}",
        ["ENABLE_FILES_LOGS"] =
            $"{nameof(LoggingSettings)}:{nameof(DiskFileLoggerSettings)}:{nameof(DiskFileLoggerSettings.Enable)}",
        ["MAX_FILES_LOGS"] =
            $"{nameof(LoggingSettings)}:{nameof(DiskFileLoggerSettings)}:{nameof(DiskFileLoggerSettings.MaxFilesToKeep)}",
        ["DEBUGGER_ENABLE"] = $"{nameof(MonoDebuggerSettings)}:{nameof(MonoDebuggerSettings.Enable)}",
        ["DEBUGGER_IP_ADDRESS"] = $"{nameof(MonoDebuggerSettings)}:{nameof(MonoDebuggerSettings.IpAddress)}",
        ["DEBUGGER_PORT"] = $"{nameof(MonoDebuggerSettings)}:{nameof(MonoDebuggerSettings.Port)}",
        ["DEBUGGER_SUSPEND_BOOT"] = $"{nameof(MonoDebuggerSettings)}:{nameof(MonoDebuggerSettings.SuspendOnBoot)}"
    };

    internal static ServiceProvider BuildServiceProvider(
        GameExecutionContext gameExecutionContext,
        string basePath,
        string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        IConfigurationManager configurationManager = new ConfigurationManager();

        FileSystem fileSystem = new();
        configurationManager.AddJsonFile(
            fileSystem.Path.Combine(basePath, "Config", "config.jsonc"));
        configurationManager.AddCustomEnvironmentVariables("VRL_", EnvironmentVariablesConfigMapping);
        configurationManager.AddCommandLine(
            args,
            EnvironmentVariablesConfigMapping
                .ToDictionary(key => $"--{key.Key.ToLower().Replace('_', '-')}", value => value.Value));

        services.AddSingleton<IConfiguration>(configurationManager);

        services.AddSingleton<IValidateOptions<GlobalSettings>, ValidateGlobalSettings>();
        services.AddOptions<GlobalSettings>()
            .BindConfiguration(string.Empty);

        // We want to get out as early as possible if needed because it prevents any other services to start
        if (configurationManager.GetValue<bool>(nameof(GlobalSettings.DisableVrl)))
            return services.BuildServiceProvider();

        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configurationManager.GetRequiredSection("Logging"));
            builder.Services.AddSingleton<ILoggerProvider, ConsoleLogProvider>();
            builder.Services.AddSingleton<ILoggerProvider, DiskFileLoggerProvider>();
        });

        services.AddSingleton<IValidateOptions<LoggingSettings>, ValidateLoggingSettings>();
        services.AddOptions<LoggingSettings>()
            .BindConfiguration(nameof(LoggingSettings), options => options.ErrorOnUnknownConfiguration = true);
        services.AddOptions<ConsoleLoggerSettings>()
            .BindConfiguration(
                $"{nameof(LoggingSettings)}:{nameof(ConsoleLoggerSettings)}",
                options => options.ErrorOnUnknownConfiguration = true);
        services.AddOptions<DiskFileLoggerSettings>()
            .BindConfiguration(
                $"{nameof(LoggingSettings)}:{nameof(DiskFileLoggerSettings)}",
                options => options.ErrorOnUnknownConfiguration = true);

        services.AddSingleton<IValidateOptions<MonoDebuggerSettings>, ValidateMonoDebuggerSettings>();
        services.AddOptions<MonoDebuggerSettings>()
            .BindConfiguration(nameof(MonoDebuggerSettings), options => options.ErrorOnUnknownConfiguration = true);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IWin32, Win32>();
        services.AddSingleton<IGameExecutionContext, GameExecutionContext>(_ => gameExecutionContext);
        services.AddSingleton<IBootstrapEnvironment, BootstrapEnvironment>(_ => new() { BasePath = basePath });

        services.AddSingleton<IPltHooksManager, PltHooksManager>(sp =>
            new PltHooksManager(sp.GetRequiredService<ILogger<PltHooksManager>>(), new PltHook(), new FileSystem()));
        services.AddSingleton<IMonoInitLifeCycleEvents, MonoInitLifeCycleEvents>();
        services.AddSingleton<StandardStreamsProtector>();

        services.AddSingleton<ICreateFileWSharedHooker, CreateFileWSharedHooker>();
        services.AddSingleton<PlayerLogsMirroring>();

        services.AddSingleton<IGlobalManagersPatcher, SplashScreenSkipper>();
        services.AddSingleton<AssembliesListAppender>();
        services.AddSingleton<IAssembliesListAppender>(x => x.GetRequiredService<AssembliesListAppender>());
        services.AddSingleton<IGlobalManagersPatcher>(x => x.GetRequiredService<AssembliesListAppender>());
        services.AddSingleton<RootGlobalManagersPatcher>();

        services.AddSingleton<IPlayerConnectionDiscovery, PlayerConnectionDiscovery>();
        services.AddSingleton<ISdbWinePathTranslator, SdbWinePathTranslator>();
        services.AddSingleton<IMonoFunctions, MonoFunctions>();
        services.AddSingleton<MonoInitializer>();

        return services.BuildServiceProvider();
    }
}