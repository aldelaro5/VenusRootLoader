using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Mono;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Unity;
using VenusRootLoader.Bootstrap.Unity.GlobalManagers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

[assembly: InternalsVisibleTo("VenusRootLoader.Bootstrap.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class contains the entrypoint method from the C++ native side, and it initialises the rest of the bootstrap
/// </summary>
internal sealed class Entry
{
    [UnmanagedCallersOnly(EntryPoint = "EntryPoint")]
    public static void EntryPoint(nint module)
    {
        ILogger? logger = null;

        try
        {
            if (!ShouldResumeEntry(out GameExecutionContext? gameExecutionContext, out string[]? args))
                return;

            SetupWindowsConsole();

            ServiceProvider serviceProvider = Startup.BuildServiceProvider(gameExecutionContext, args);
            IOptions<GlobalSettings>? globalSettings = serviceProvider.GetService<IOptions<GlobalSettings>>();
            if (globalSettings!.Value.DisableVrl!.Value)
                return;

            // Since we have to attach a new console window due to loader lock restrictions,
            // we have to always attach one, but hide it until we find that it should be shown.
            // In case we didn't want the console, we simply detach it so no logging can happen to it
            IOptions<LoggingSettings>? loggingSettings = serviceProvider.GetService<IOptions<LoggingSettings>>();
            if (loggingSettings!.Value.ConsoleLoggerSettings.Enable!.Value)
                PInvoke.ShowWindow(PInvoke.GetConsoleWindow(), SHOW_WINDOW_CMD.SW_SHOW);
            else
                PInvoke.FreeConsole();

            ManagedLogsRelay.Init(serviceProvider.GetRequiredService<ILoggerFactory>());

            logger = serviceProvider.GetRequiredService<ILogger<Entry>>();
            logger.LogInformation("Using base directory {BaseDir}", gameExecutionContext.BaseDir);

            StandardStreamsProtector standardStreamsProtector =
                serviceProvider.GetRequiredService<StandardStreamsProtector>();
            standardStreamsProtector.ProtectStreams();

            PlayerLogsMirroring playerLogsMirroring = serviceProvider.GetRequiredService<PlayerLogsMirroring>();
            playerLogsMirroring.MirrorLogs();

            RootGlobalManagersPatcher rootGlobalManagersPatcher =
                serviceProvider.GetRequiredService<RootGlobalManagersPatcher>();
            rootGlobalManagersPatcher.SetupPatchers();

            MonoInitializer monoInitializer =
                serviceProvider.GetRequiredService<MonoInitializer>();
            monoInitializer.HookMonoInitialization();

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
        [NotNullWhen(true)] out GameExecutionContext? gameExecutionContext,
        [NotNullWhen(true)] out string[]? args)
    {
        FileSystem fileSystem = new();
        string exePath = Environment.ProcessPath!;
        string gameDir = fileSystem.Path.GetDirectoryName(exePath)!;
        string dataDir = fileSystem.Path.Combine(
            gameDir,
            fileSystem.Path.GetFileNameWithoutExtension(exePath) + "_Data");

        // It's technically possible another process residing outside the game's directory ends up right back
        // here even after the initialisation happened. This heuristic protects from that by making sure we are
        // in the game's directory
        if (!fileSystem.Directory.Exists(dataDir))
        {
            gameExecutionContext = null;
            args = null;
            return false;
        }

        string unityPlayerDllFileName = Process.GetCurrentProcess().Modules
            .OfType<ProcessModule>()
            .Single(x => x.FileName.Contains("UnityPlayer")).FileName;

        FreeLibrarySafeHandle hModNtDll = PInvoke.GetModuleHandle("ntdll.dll");
        FARPROC wineGetVersion = PInvoke.GetProcAddress(hModNtDll, "wine_get_version");
        bool isWine = wineGetVersion != FARPROC.Null;
        List<string> sanitisedArgs = SanitiseCommandLineArguments();
        string? customContentRootPath = SetCustomContentRootPathIfProvided(sanitisedArgs);

        gameExecutionContext = new()
        {
            GameDir = gameDir,
            DataDir = dataDir,
            UnityPlayerDllFileName = unityPlayerDllFileName,
            IsWine = isWine,
            BaseDir = customContentRootPath ?? gameDir
        };

        args = sanitisedArgs.ToArray();
        return true;
    }

    private static List<string> SanitiseCommandLineArguments()
    {
        string[] args = Environment.GetCommandLineArgs();
        List<string> sanitisedArgs = new();
        for (int i = 0; i < args.Length - 1; i++)
        {
            string arg = args[i];
            if (!arg.StartsWith("--"))
                continue;

            sanitisedArgs.Add(arg);
            sanitisedArgs.Add(args[i + 1]);
        }

        return sanitisedArgs;
    }

    [SuppressMessage(
        "System.IO.Abstractions",
        "IO0003:Replace Directory class with IFileSystem.Directory for improved testability")]
    private static string? SetCustomContentRootPathIfProvided(
        List<string> args)
    {
        string? customBasePath = null;
        string? baseDirEnv = Environment.GetEnvironmentVariable("VRL_BASE_DIRECTORY");
        if (!string.IsNullOrWhiteSpace(baseDirEnv) && Directory.Exists(baseDirEnv))
            customBasePath = baseDirEnv;

        int baseDirArgIndex = args.IndexOf("--base-directory");
        if (baseDirArgIndex == -1 || baseDirArgIndex + 1 >= args.Count)
            return customBasePath;

        if (!string.IsNullOrWhiteSpace(args[baseDirArgIndex + 1]))
            customBasePath = args[baseDirArgIndex + 1];

        return customBasePath;
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