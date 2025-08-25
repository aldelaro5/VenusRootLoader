using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap;

internal class Entry
{
    public static nint LibraryHandle { get; private set; }
    public static string GameDir { get; private set; } = null!;
    public static string DataDir { get; private set; } = null!;
    public static string PlayerFileName { get; private set; } = null!;

    [UnmanagedCallersOnly(EntryPoint = "EntryPoint")]
    public static void EntryPoint(nint module)
    {
        LibraryHandle = module;

        var exePath = Environment.ProcessPath!;
        GameDir = Path.GetDirectoryName(exePath)!;

        DataDir = Path.Combine(GameDir, Path.GetFileNameWithoutExtension(exePath) + "_Data");

        if (!Directory.Exists(DataDir))
            return;

        PlayerFileName = Process.GetCurrentProcess().Modules
            .OfType<ProcessModule>()
            .Single(x => x.FileName.Contains("UnityPlayer")).FileName;

        WindowsConsole.BindToGame();
        UnityPlayerLogsMirroring.SetupPlayerLogMirroring();
        Console.WriteLine("Hi!");
    }
}