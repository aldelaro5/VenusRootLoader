using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Extensions;
using VenusRootLoader.Bootstrap.HostedServices;
using VenusRootLoader.Bootstrap.HostedServices.Runtime;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Services;

namespace VenusRootLoader.Bootstrap;

internal static class Startup
{
    private static readonly MonoInitializer.ManagedEntryPointInfo ManagedEntryPointInfo = new()
    {
        AssemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "VenusRootLoader", "VenusRootLoader.dll"),
        Namespace = "VenusRootLoader",
        ClassName = "MonoInitEntry",
        MethodName = "Main"
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
            Configuration = null
        });

        builder.Services.AddSingleton<GameExecutionContext>(_ => gameExecutionContext);
        builder.Services.AddHostedService<WindowsConsole>();
        builder.Services.AddSingleton<PltHook>();
        builder.Services.AddHostedService<StandardStreamsProtector>();
        builder.Services.AddSingleton<ILoggerFactory>(provider =>
            LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.AddConsoleLoggingProvider(provider);
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            }));
        builder.Services.AddSingleton<CreateFileWSharedHooker>();
        builder.Services.AddHostedService<UnityPlayerLogsMirroring>();
        builder.Services.AddHostedService<UnitySplashScreenSkipper>();
        builder.Services.AddHostedService<MonoInitializer>(s => new(
            s.GetRequiredService<ILogger<MonoInitializer>>(),
            s.GetRequiredService<PltHook>(),
            s.GetRequiredService<GameExecutionContext>(),
            ManagedEntryPointInfo));

        return builder.Build();
    }
}