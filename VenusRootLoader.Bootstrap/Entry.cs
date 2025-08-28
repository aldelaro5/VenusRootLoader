using System.Diagnostics;
using System.Runtime.InteropServices;

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
        UnityPlayerLogsMirroring.SetUp();
        Console.WriteLine("Bootstrapping Mono...");
        MonoInitializer.Setup(new()
        {
            AssemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "VenusRootLoader", "VenusRootLoader.dll"),
            Namespace = "VenusRootLoader",
            ClassName = "MonoInitEntry",
            MethodName = "Main"
        });
        Console.WriteLine("Resuming UnityMain");
    }
}