using System.Runtime.InteropServices;
using System.Text;

namespace VenusRootLoader.Bootstrap;

internal class WindowsConsole
{
    internal static nint OutputHandle;
    internal static nint ErrorHandle;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CloseHandleFn(nint hObject);
    private static readonly CloseHandleFn HookCloseHandleDelegate = HookCloseHandle;
    
    public static void BindToGame()
    {
        var consoleWindow = WindowsNative.GetConsoleWindow();
        var stdOut = WindowsNative.GetStdHandle(WindowsNative.StdOutputHandle);
        // var stdErr = WindowsNative.GetStdHandle(WindowsNative.StdErrorHandle);
        
        var isWine = NativeLibrary.TryLoad("ntdll.dll", out var ntdll) &&
                     NativeLibrary.TryGetExport(ntdll, "wine_get_version", out _);

        // Do not create a new window if a window already exists or the output is being redirected.
        // On Wine, we always want to show the window because it's possible the handle isn't null due to Wine itself
        if (consoleWindow == 0 && (stdOut == 0 || isWine))
        {
            WindowsNative.AllocConsole();
            consoleWindow = WindowsNative.GetConsoleWindow();
            if (consoleWindow == 0)
                return;

            // TODO: The following line set the console windows on top, do we want this as a setting still?
            // WindowsNative.SetWindowPos(consoleWindow, -1, 0, 0, 0, 0, 0x0001 | 0x0002);
        }

        PltHook.InstallHook(Entry.PlayerFileName, "CloseHandle", Marshal.GetFunctionPointerForDelegate(HookCloseHandleDelegate));
        
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));

        Console.OutputEncoding = Encoding.UTF8;

        OutputHandle = WindowsNative.GetStdHandle(WindowsNative.StdOutputHandle);
        ErrorHandle = WindowsNative.GetStdHandle(WindowsNative.StdErrorHandle);
        Console.WriteLine($"stdout before console: {stdOut}");
        Console.WriteLine($"stdout: {OutputHandle:X8}");
        Console.WriteLine($"stderr: {ErrorHandle:X8}");
    }

    private static int HookCloseHandle(nint hObject)
    {
        // Console.WriteLine($"HookCloseHandle: {hObject:X8}");
        if (hObject != OutputHandle && hObject != ErrorHandle)
            return WindowsNative.CloseHandle(hObject);

        Console.WriteLine($"Prevented the CloseHandle of {(hObject == OutputHandle ? "stdout" : "stderr")}");
        return 1;
    }
}