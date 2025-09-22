using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap.Shared;

public interface IPltHook
{
    public bool PlthookOpen(ref nint pltHookOut, string? filename);
    public bool PlthookReplace(nint pltHook, string funcName, nint funcAddr, ref nint oldFunc);
    public void PlthookClose(nint pltHook);
    public nint PlthookError();
}

public partial class PltHook : IPltHook
{
    [LibraryImport("*", EntryPoint = "plthook_open", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int PinvokePlthookOpen(ref nint pltHookOut, string? filename);

    [LibraryImport("*", EntryPoint = "plthook_replace", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int PinvokePlthookReplace(nint pltHook, string funcName, nint funcAddr, ref nint oldFunc);

    [LibraryImport("*", EntryPoint = "plthook_close")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void PinvokePlthookClose(nint pltHook);

    [LibraryImport("*", EntryPoint = "plthook_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint PinvokePlthookError();

    public bool PlthookOpen(ref nint pltHookOut, string? filename) => PinvokePlthookOpen(ref pltHookOut, filename) == 0;
    public bool PlthookReplace(nint pltHook, string funcName, nint funcAddr, ref nint oldFunc) => PinvokePlthookReplace(pltHook, funcName, funcAddr, ref oldFunc) == 0;
    public void PlthookClose(nint pltHook) => PinvokePlthookClose(pltHook);
    public nint PlthookError() => PinvokePlthookError();
}