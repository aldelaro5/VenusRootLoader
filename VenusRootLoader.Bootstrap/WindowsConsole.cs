using System.Runtime.InteropServices;
using System.Text;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class handles the binding of Windows's console for usage in our logs. It also prevents the console's streams to
/// be closed by Unity
/// </summary>
internal static class WindowsConsole
{
    internal static nint OutputHandle;
    internal static nint ErrorHandle;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CloseHandleFn(nint hObject);
    private static readonly CloseHandleFn HookCloseHandleDelegate = HookCloseHandle;

    public static void SetUp()
    {
        // The actual logic that creates the console if needed is done on the C++ side because it is required to perform
        // this logic during DllMain under a loader lock due to the need to do this before UnityPlayer.dll's CRT initialisation.
        // Since it's not possible to initialise the bootstrap under loader lock as of .NET 10, the console's creation
        // has to be handled on the C++ side
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
    private static int HookCloseHandle(nint hObject)
    {
        if (hObject == OutputHandle || hObject == ErrorHandle)
        {
            Console.WriteLine($"Prevented the CloseHandle of {(hObject == OutputHandle ? "stdout" : "stderr")}");
            return 1;
        }

        return WindowsNative.CloseHandle(hObject);
    }
}