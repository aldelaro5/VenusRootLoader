using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

[assembly: InternalsVisibleTo("VenusRootLoader.Bootstrap.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class contains the entrypoint method from the C++ native side, and it initialises the rest of the bootstrap
/// </summary>
internal class Entry
{
    [UnmanagedCallersOnly(EntryPoint = "EntryPoint")]
    public static void EntryPoint(nint module)
    {
        ILogger? logger = null;

        try
        {
            if (!ShouldResumeEntry(module, out var gameExecutionContext))
                return;

            SetupWindowsConsole();

            var host = Startup.BuildHost(gameExecutionContext);
            var globalSettings = host.Services.GetService<IOptions<GlobalSettings>>();
            if (globalSettings!.Value.DisableVrl!.Value)
                return;

            var loggingSettings = host.Services.GetService<IOptions<LoggingSettings>>();
            if (loggingSettings!.Value.ConsoleLoggerSettings.Enable!.Value)
                PInvoke.ShowWindow(PInvoke.GetConsoleWindow(), SHOW_WINDOW_CMD.SW_SHOW);
            else
                PInvoke.FreeConsole();

            logger = host.Services.GetRequiredService<ILogger<Entry>>();
            var environment = host.Services.GetRequiredService<IHostEnvironment>();
            logger.LogInformation("Using base directory {EnvironmentContentRootPath}", environment.ContentRootPath);
            host.Start();
            logger.LogInformation("Resuming UnityMain");
        }
        catch (Exception ex)
        {
            logger?.LogCritical(ex, "An unhandled exception occurred during the entrypoint");
            PInvoke.MessageBox(HWND.Null, ex.ToString(), "Unhandled Exception", MESSAGEBOX_STYLE.MB_ICONERROR);
            throw;
        }
    }

    private static bool ShouldResumeEntry(
        IntPtr module,
        [NotNullWhen(true)] out GameExecutionContext? gameExecutionContext)
    {
        var libraryHandle = module;
        var fileSystem = new FileSystem();
        var exePath = Environment.ProcessPath!;
        var gameDir = fileSystem.Path.GetDirectoryName(exePath)!;
        var dataDir = fileSystem.Path.Combine(gameDir, fileSystem.Path.GetFileNameWithoutExtension(exePath) + "_Data");

        // It's technically possible another process residing outside the game's directory ends up right back
        // here even after the initialisation happened. This heuristic protects from that by making sure we are
        // in the game's directory
        if (!fileSystem.Directory.Exists(dataDir))
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

    private static void SetupWindowsConsole()
    {
        // The actual logic that creates the console if needed is done on the C++ side because it is required to perform
        // this logic during DllMain under a loader lock due to the need to do this before UnityPlayer.dll's CRT initialisation.
        // Since it's not possible to initialise the bootstrap under loader lock as of .NET 10, the console's creation
        // has to be handled on the C++ side
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));

        Console.OutputEncoding = Encoding.UTF8;
    }
}