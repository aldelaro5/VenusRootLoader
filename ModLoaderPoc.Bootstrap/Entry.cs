using System.Runtime.InteropServices;

namespace ModLoaderPoc.Bootstrap;

public partial class Entry
{
    private const uint MbOk = 0x0;
    private const uint MbIconInformation = 0x40;
    
    [LibraryImport("user32.dll", EntryPoint = "MessageBoxA", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void MessageBoxA(nint hWnd, string text, string caption, uint uType);
    
    [UnmanagedCallersOnly(EntryPoint = "EntryPoint")]
    public static void EntryPoint(nint module)
    {
        MessageBoxA(nint.Zero, "Hello World!", "Hello", MbOk | MbIconInformation);
    }
}