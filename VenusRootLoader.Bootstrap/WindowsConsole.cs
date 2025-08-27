using System.Runtime.InteropServices;
using System.Text;

namespace VenusRootLoader.Bootstrap;

internal static class WindowsConsole
{
    internal static nint OutputHandle;
    internal static nint ErrorHandle;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CloseHandleFn(uint hObject);
    private static readonly CloseHandleFn HookCloseHandleDelegate = HookCloseHandle;

    public static void SetUp()
    {
        // By this point, AllocConsole was already called if needed on the C++ side which means these calls
        // are guaranteed to bind the managed System.Console to the Win32 console and allow us to use it as normal
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));

        Console.OutputEncoding = Encoding.UTF8;

        OutputHandle = WindowsNative.GetStdHandle(WindowsNative.StdOutputHandle);
        ErrorHandle = WindowsNative.GetStdHandle(WindowsNative.StdErrorHandle);

        PltHook.InstallHook(Entry.UnityPlayerDllFileName, "CloseHandle", Marshal.GetFunctionPointerForDelegate(HookCloseHandleDelegate));
    }

    // Unity may attempt to close stdout and stderr in order to redirect their streams to their player logs.
    // Since we attempt to control all logging, we want to prevent this from happening which is what this hook is for
    private static int HookCloseHandle(uint hObject)
    {
        if (hObject == OutputHandle || hObject == ErrorHandle)
        {
            Console.WriteLine($"Prevented the CloseHandle of {(hObject == OutputHandle ? "stdout" : "stderr")}");
            return 1;
        }

        return WindowsNative.CloseHandle(hObject);
    }
}