using System.Text;

namespace VenusRootLoader.Bootstrap;

internal class WindowsConsole
{
    internal static nint OutputHandle;
    internal static nint ErrorHandle;

    public static void BindToGame()
    {
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));

        Console.OutputEncoding = Encoding.UTF8;

        OutputHandle = WindowsNative.GetStdHandle(WindowsNative.StdOutputHandle);
        ErrorHandle = WindowsNative.GetStdHandle(WindowsNative.StdErrorHandle);
    }
}