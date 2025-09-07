using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Extensions;
using VenusRootLoader.Bootstrap.HostedServices;
using VenusRootLoader.Bootstrap.HostedServices.Runtime;
using VenusRootLoader.Bootstrap.Services;
using VenusRootLoader.Bootstrap.Settings;

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

        var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "VenusRootLoader", "VenusRootLoader.dll");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ManagedEntryPointInfo:AssemblyPath"] = assemblyPath,
            ["ManagedEntryPointInfo:Namespace"] = "VenusRootLoader",
            ["ManagedEntryPointInfo:ClassName"] = "MonoInitEntry",
            ["ManagedEntryPointInfo:MethodName"] = "Main"
        });
        builder.Configuration.AddJsonFile(Path.Combine("Config", "config.jsonc"));
        builder.Configuration.AddJsonFile(Path.Combine("Config", "boot.jsonc"));

        builder.Logging.AddConfiguration(builder.Configuration.GetRequiredSection("Logging"));
        builder.Logging.AddConsoleLoggingProvider();
        if (!builder.Configuration.GetValue<bool>("LoggingSettings:DisableUnityLogs"))
            builder.Logging.AddFilter("UNITY", LogLevel.Trace);

        builder.Services.AddSingleton<IValidateOptions<GlobalSettings>, ValidateGlobalSettings>();
        builder.Services.AddOptions<GlobalSettings>()
            .BindConfiguration(string.Empty);
        builder.Services.AddSingleton<IValidateOptions<LoggingSettings>, ValidateLoggingSettings>();
        builder.Services.AddOptions<LoggingSettings>()
            .BindConfiguration(nameof(LoggingSettings), options => options.ErrorOnUnknownConfiguration = true);
        builder.Services.AddSingleton<IValidateOptions<MonoDebuggerSettings>, ValidateMonoDebuggerSettings>();
        builder.Services.AddOptions<MonoDebuggerSettings>()
            .BindConfiguration(nameof(MonoDebuggerSettings), options => options.ErrorOnUnknownConfiguration = true);
        builder.Services.AddOptions<BootConfigSettings>()
            .Bind(builder.Configuration.GetRequiredSection(nameof(BootConfigSettings)));
        builder.Services.AddSingleton<IValidateOptions<ManagedEntryPointInfo>, ValidateManagedEntryPointInfoOptions>();
        builder.Services.AddOptions<ManagedEntryPointInfo>()
            .BindConfiguration(nameof(ManagedEntryPointInfo), options => options.ErrorOnUnknownConfiguration = true);

        builder.Services.AddSingleton<GameExecutionContext>(_ => gameExecutionContext);
        builder.Services.AddSingleton<PltHook>();
        builder.Services.AddHostedService<StandardStreamsProtector>();
        builder.Services.AddSingleton<CreateFileWSharedHooker>();
        builder.Services.AddHostedService<UnityPlayerLogsMirroring>();
        builder.Services.AddHostedService<UnitySplashScreenSkipper>();
        builder.Services.AddHostedService<UnityBootConfigCustomizer>();
        builder.Services.AddSingleton<UnityPlayerConnectionDiscovery>();
        builder.Services.AddHostedService<MonoInitializer>();

        return builder.Build();
    }
}