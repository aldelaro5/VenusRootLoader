using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class contains the entrypoint method from the C++ side, and it initialises the rest of the bootstrap
/// </summary>
internal static class Entry
{
    public static nint LibraryHandle { get; private set; }
    public static string GameDir { get; private set; } = null!;
    public static string DataDir { get; private set; } = null!;
    public static string UnityPlayerDllFileName { get; private set; } = null!;

    private static readonly MonoInitializer.ManagedEntryPointInfo ManagedEntryPointInfo = new()
    {
        AssemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "VenusRootLoader", "VenusRootLoader.dll"),
        Namespace = "VenusRootLoader",
        ClassName = "MonoInitEntry",
        MethodName = "Main"
    };

    [UnmanagedCallersOnly(EntryPoint = "EntryPoint")]
    public static void EntryPoint(nint module)
    {
        LibraryHandle = module;

        var exePath = Environment.ProcessPath!;
        GameDir = Path.GetDirectoryName(exePath)!;

        DataDir = Path.Combine(GameDir, Path.GetFileNameWithoutExtension(exePath) + "_Data");

        // It's technically possible another process residing outside the game's directory ends up right back
        // here even after the initialisation happened. This heuristic protects from that by making sure we are
        // in the game's directory
        if (!Directory.Exists(DataDir))
            return;

        UnityPlayerDllFileName = Process.GetCurrentProcess().Modules
            .OfType<ProcessModule>()
            .Single(x => x.FileName.Contains("UnityPlayer")).FileName;

        WindowsConsole.SetUp();

        var builder = Host.CreateEmptyApplicationBuilder(new()
        {
            DisableDefaults = true,
            ApplicationName = "VenusRootLoader",
            Args = [],
            EnvironmentName = "Development",
            ContentRootPath = GameDir,
            Configuration = null
        });
        builder.Services.AddSingleton<ILoggerFactory>(_ =>
            LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.AddConsoleLoggingProvider();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            }));
        builder.Services.AddHostedService<FileHandleHook>();
        builder.Services.AddHostedService<UnityPlayerLogsMirroring>();
        builder.Services.AddHostedService<UnitySplashScreenSkipper>();
        builder.Services.AddHostedService<MonoInitializer>(s => new(
            s.GetRequiredService<ILoggerFactory>(),
            ManagedEntryPointInfo));
        var host = builder.Build();

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(Entry), Color.Magenta);

        try
        {
            host.Start();
            logger.LogInformation("Resuming UnityMain");
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "An unhandled exception occurred during the entrypoint");
            WindowsNative.MessageBoxW(nint.Zero, e.ToString(), "Unhandled Exception", 0x10);
            throw;
        }
    }
}