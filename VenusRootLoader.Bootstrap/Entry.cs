using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Extensions;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class contains the entrypoint method from the C++ side, and it initialises the rest of the bootstrap
/// </summary>
internal static class Entry
{
    [UnmanagedCallersOnly(EntryPoint = "EntryPoint")]
    public static void EntryPoint(nint module)
    {
        var libraryHandle = module;
        var exePath = Environment.ProcessPath!;
        var gameDir = Path.GetDirectoryName(exePath)!;
        var dataDir = Path.Combine(gameDir, Path.GetFileNameWithoutExtension(exePath) + "_Data");

        // It's technically possible another process residing outside the game's directory ends up right back
        // here even after the initialisation happened. This heuristic protects from that by making sure we are
        // in the game's directory
        if (!Directory.Exists(dataDir))
            return;

        var unityPlayerDllFileName = Process.GetCurrentProcess().Modules
            .OfType<ProcessModule>()
            .Single(x => x.FileName.Contains("UnityPlayer")).FileName;

        var host = Startup.BuildHost(gameDir, libraryHandle, dataDir, unityPlayerDllFileName);

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(Entry), Color.Magenta);

        try
        {
            host.Start();
            logger.LogInformation("Resuming UnityMain");
        }
        catch (Exception ex)
        {
            // logger.LogCritical(ex, "An unhandled exception occurred during the entrypoint");
            PInvoke.MessageBox(HWND.Null, ex.ToString(), "Unhandled Exception", MESSAGEBOX_STYLE.MB_ICONERROR);
            throw;
        }
    }
}