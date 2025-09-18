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
using PltHook = VenusRootLoader.Bootstrap.Shared.PltHook;
using ValidateLoggingSettings = VenusRootLoader.Bootstrap.Settings.ValidateLoggingSettings;

namespace VenusRootLoader.Bootstrap;

internal static class Startup
{
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

        builder.Configuration.AddJsonFile(Path.Combine("Config", "config.jsonc"));
        builder.Configuration.AddJsonFile(Path.Combine("Config", "boot.jsonc"));

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

        builder.Services.AddSingleton<GameExecutionContext>(_ => gameExecutionContext);
        builder.Services.AddSingleton<PltHook>();
        builder.Services.AddSingleton<GameLifecycleEvents>();
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
}