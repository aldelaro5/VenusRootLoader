using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Extensions;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Mono;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Settings.LogProvider;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Unity;
using ValidateLoggingSettings = VenusRootLoader.Bootstrap.Settings.ValidateLoggingSettings;

namespace VenusRootLoader.Bootstrap;

internal static class Startup
{
    private static readonly Dictionary<string, string> EnvironmentVariablesConfigMapping = new()
    {
        ["INCLUDE_UNITY_LOGS"] = $"{nameof(LoggingSettings)}:{nameof(LoggingSettings.IncludeUnityLogs)}",
        ["ENABLE_CONSOLE_LOGS"] = $"{nameof(LoggingSettings)}:{nameof(ConsoleLoggerSettings)}:{nameof(ConsoleLoggerSettings.Enable)}",
        ["CONSOLE_COLORS"] = $"{nameof(LoggingSettings)}:{nameof(ConsoleLoggerSettings)}:{nameof(ConsoleLoggerSettings.LogWithColors)}",
        ["ENABLE_FILES_LOGS"] = $"{nameof(LoggingSettings)}:{nameof(DiskFileLoggerSettings)}:{nameof(DiskFileLoggerSettings.Enable)}",
        ["MAX_FILES_LOGS"] = $"{nameof(LoggingSettings)}:{nameof(DiskFileLoggerSettings)}:{nameof(DiskFileLoggerSettings.MaxFilesToKeep)}",
        ["DEBUGGER_ENABLE"] = $"{nameof(MonoDebuggerSettings)}:{nameof(MonoDebuggerSettings.Enable)}",
        ["DEBUGGER_IP_ADDRESS"] = $"{nameof(MonoDebuggerSettings)}:{nameof(MonoDebuggerSettings.IpAddress)}",
        ["DEBUGGER_PORT"] = $"{nameof(MonoDebuggerSettings)}:{nameof(MonoDebuggerSettings.Port)}",
        ["DEBUGGER_SUSPEND_BOOT"] = $"{nameof(MonoDebuggerSettings)}:{nameof(MonoDebuggerSettings.SuspendOnBoot)}"
    };

    internal static IHost BuildHost(GameExecutionContext gameExecutionContext)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new()
        {
            DisableDefaults = true,
            ApplicationName = "VenusRootLoader",
            Args = [],
            EnvironmentName = "Development",
            ContentRootPath = gameExecutionContext.GameDir,
            Configuration = new()
        });

        var fileSystem = new FileSystem();
        var sanitisedArgs = SanitiseCommandLineArguments();
        SetCustomContentRootPathIfProvided(builder.Environment, sanitisedArgs, fileSystem);

        builder.Configuration.AddJsonFile(fileSystem.Path.Combine(builder.Environment.ContentRootPath, "Config", "config.jsonc"));
        builder.Configuration.AddJsonFile(fileSystem.Path.Combine(builder.Environment.ContentRootPath, "Config", "boot.jsonc"));
        builder.Configuration.AddCustomEnvironmentVariables("VRL_", EnvironmentVariablesConfigMapping);
        builder.Configuration.AddCommandLine(sanitisedArgs.ToArray(), EnvironmentVariablesConfigMapping
            .ToDictionary(key => $"--{key.Key.ToLower().Replace('_', '-')}", value => value.Value));

        builder.Logging.AddConfiguration(builder.Configuration.GetRequiredSection("Logging"));
        builder.Logging.AddConsoleLoggingProvider();
        builder.Logging.AddFileLoggingProvider();
        if (!builder.Configuration.GetValue<bool>("LoggingSettings:DisableUnityLogs"))
            builder.Logging.AddFilter("UNITY", LogLevel.Trace);

        builder.Services.AddSingleton<IValidateOptions<GlobalSettings>, ValidateGlobalSettings>();
        builder.Services.AddOptions<GlobalSettings>()
            .BindConfiguration(string.Empty);
        builder.Services.AddSingleton<IValidateOptions<LoggingSettings>, ValidateLoggingSettings>();
        builder.Services.AddOptions<LoggingSettings>()
            .BindConfiguration(nameof(LoggingSettings), options => options.ErrorOnUnknownConfiguration = true);
        builder.Services.AddOptions<ConsoleLoggerSettings>()
            .BindConfiguration($"{nameof(LoggingSettings)}:{nameof(ConsoleLoggerSettings)}",
                options => options.ErrorOnUnknownConfiguration = true);
        builder.Services.AddOptions<DiskFileLoggerSettings>()
            .BindConfiguration($"{nameof(LoggingSettings)}:{nameof(DiskFileLoggerSettings)}",
                options => options.ErrorOnUnknownConfiguration = true);

        builder.Services.AddSingleton<IValidateOptions<MonoDebuggerSettings>, ValidateMonoDebuggerSettings>();
        builder.Services.AddOptions<MonoDebuggerSettings>()
            .BindConfiguration(nameof(MonoDebuggerSettings), options => options.ErrorOnUnknownConfiguration = true);
        builder.Services.AddOptions<BootConfigSettings>()
            .Bind(builder.Configuration.GetRequiredSection(nameof(BootConfigSettings)));

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IWin32, Win32>();
        builder.Services.AddSingleton<GameExecutionContext>(_ => gameExecutionContext);
        builder.Services.AddSingleton<IPltHooksManager ,PltHooksManager>(sp => 
            new PltHooksManager(sp.GetRequiredService<ILogger<PltHooksManager>>(), new PltHook(), new FileSystem()));
        builder.Services.AddSingleton<IGameLifecycleEvents, GameLifecycleEvents>();
        builder.Services.AddHostedService<StandardStreamsProtector>();
        builder.Services.AddSingleton<CreateFileWSharedHooker>();
        builder.Services.AddHostedService<PlayerLogsMirroring>();
        builder.Services.AddHostedService<SplashScreenSkipper>();
        builder.Services.AddHostedService<BootConfigCustomizer>();
        builder.Services.AddSingleton<PlayerConnectionDiscovery>();
        builder.Services.AddSingleton<SdbWinePathTranslator>();
        builder.Services.AddHostedService<MonoInitializer>();

        return builder.Build();
    }

    private static void SetCustomContentRootPathIfProvided(IHostEnvironment hostEnvironment, List<string> args,
        FileSystem fileSystem)
    {
        var baseDirEnv = Environment.GetEnvironmentVariable("VRL_BASE_DIRECTORY");
        if (!string.IsNullOrWhiteSpace(baseDirEnv) && fileSystem.Directory.Exists(baseDirEnv))
            hostEnvironment.ContentRootPath = baseDirEnv;

        var baseDirArgIndex = args.IndexOf("--base-directory");
        if (baseDirArgIndex == -1)
            return;
        if (baseDirArgIndex + 1 >= args.Count)
            return;
        if (!string.IsNullOrWhiteSpace(args[baseDirArgIndex + 1]))
            hostEnvironment.ContentRootPath = args[baseDirArgIndex + 1];
    }

    private static List<string> SanitiseCommandLineArguments()
    {
        var args = Environment.GetCommandLineArgs();
        List<string> sanitisedArgs = new();
        for (int i = 0; i < args.Length - 1; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--"))
                continue;

            sanitisedArgs.Add(arg);
            sanitisedArgs.Add(args[i + 1]);
        }

        return sanitisedArgs;
    }
}