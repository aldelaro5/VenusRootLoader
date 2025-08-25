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
        // TODO: The following line set the console windows on top, do we want this as a setting still?
        // WindowsNative.SetWindowPos(consoleWindow, -1, 0, 0, 0, 0, 0x0001 | 0x0002);

        PltHook.InstallHook(Entry.PlayerFileName, "CloseHandle", Marshal.GetFunctionPointerForDelegate(HookCloseHandleDelegate));
        
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));

        Console.OutputEncoding = Encoding.UTF8;

        OutputHandle = WindowsNative.GetStdHandle(WindowsNative.StdOutputHandle);
        ErrorHandle = WindowsNative.GetStdHandle(WindowsNative.StdErrorHandle);
    }

    private static int HookCloseHandle(nint hObject)
    {
        if (hObject != OutputHandle && hObject != ErrorHandle)
            return WindowsNative.CloseHandle(hObject);

        Console.WriteLine($"Prevented the CloseHandle of {(hObject == OutputHandle ? "stdout" : "stderr")}");
        return 1;
    }
}