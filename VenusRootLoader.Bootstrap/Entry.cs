using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Extensions;
using VenusRootLoader.Bootstrap.Services;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class contains the entrypoint method from the C++ side, and it initialises the rest of the bootstrap
/// </summary>
internal static class Entry
{
    [UnmanagedCallersOnly(EntryPoint = "EntryPoint")]
    public static void EntryPoint(nint module)
    {
        if (!ShouldResumeEntry(module, out var gameExecutionContext))
            return;

        var host = Startup.BuildHost(gameExecutionContext);

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

    private static bool ShouldResumeEntry(IntPtr module, [NotNullWhen(true)] out GameExecutionContext? gameExecutionContext)
    {
        var libraryHandle = module;
        var exePath = Environment.ProcessPath!;
        var gameDir = Path.GetDirectoryName(exePath)!;
        var dataDir = Path.Combine(gameDir, Path.GetFileNameWithoutExtension(exePath) + "_Data");

        // It's technically possible another process residing outside the game's directory ends up right back
        // here even after the initialisation happened. This heuristic protects from that by making sure we are
        // in the game's directory
        if (!Directory.Exists(dataDir))
        {
            gameExecutionContext = null;
            return false;
        }

        var unityPlayerDllFileName = Process.GetCurrentProcess().Modules
            .OfType<ProcessModule>()
            .Single(x => x.FileName.Contains("UnityPlayer")).FileName;

        var hModNtDll = PInvoke.GetModuleHandle("ntdll.dll");
        var wineGetVersion = PInvoke.GetProcAddress(hModNtDll, "wine_get_version");
        var isWine = wineGetVersion != FARPROC.Null;

        gameExecutionContext = new()
        {
            LibraryHandle = libraryHandle,
            GameDir = gameDir,
            DataDir = dataDir,
            UnityPlayerDllFileName = unityPlayerDllFileName,
            IsWine = isWine
        };
        return true;
    }
}