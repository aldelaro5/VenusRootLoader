using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Extensions;
using VenusRootLoader.Bootstrap.HostedServices;
using VenusRootLoader.Bootstrap.HostedServices.Runtime;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Services;

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
        builder.Services.AddSingleton<GameExecutionContext>(_ => gameExecutionContext);
        builder.Services.AddHostedService<WindowsConsole>();
        builder.Services.AddSingleton<PltHook>();
        builder.Services.AddHostedService<StandardStreamsProtector>();
        builder.Logging.AddConsoleLoggingProvider();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Services.AddSingleton<CreateFileWSharedHooker>();
        builder.Services.AddHostedService<UnityPlayerLogsMirroring>();
        builder.Services.AddHostedService<UnitySplashScreenSkipper>();
        builder.Services.AddSingleton<IValidateOptions<ManagedEntryPointInfo>, ValidateManagedEntryPointInfoOptions>();
        builder.Services.AddOptions<ManagedEntryPointInfo>()
            .BindConfiguration(nameof(ManagedEntryPointInfo));
        builder.Services.AddHostedService<MonoInitializer>();

        return builder.Build();
    }
}