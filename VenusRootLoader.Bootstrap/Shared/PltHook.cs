using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap.Shared;

/// <summary>
/// An abstracted interface of all PltHook functions we use so they can easily be mocked in unit tests
/// </summary>
public interface IPltHook
{
    bool PlthookOpen(Pointer<nint> pltHookOut, string? filename);
    bool PlthookReplace(nint pltHook, string funcName, nint funcAddr, Pointer<nint> oldFunc);
    void PlthookClose(nint pltHook);
    nint PlthookError();
}

/// <summary>
/// The real implementation of <see cref="IPltHook"/> that calls the real functions with PInvoke
/// </summary>
public partial class PltHook : IPltHook
{
    [LibraryImport("*", EntryPoint = "plthook_open", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int PinvokePlthookOpen(nint* pltHookOut, string? filename);

    [LibraryImport("*", EntryPoint = "plthook_replace", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int PinvokePlthookReplace(
        nint pltHook,
        string funcName,
        nint funcAddr,
        nint* oldFunc);

    [LibraryImport("*", EntryPoint = "plthook_close")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void PinvokePlthookClose(nint pltHook);

    [LibraryImport("*", EntryPoint = "plthook_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint PinvokePlthookError();

    public unsafe bool PlthookOpen(Pointer<nint> pltHookOut, string? filename) =>
        PinvokePlthookOpen(pltHookOut.Value, filename) == 0;

    public unsafe bool PlthookReplace(nint pltHook, string funcName, nint funcAddr, Pointer<nint> oldFunc) =>
        PinvokePlthookReplace(pltHook, funcName, funcAddr, oldFunc.Value) == 0;

    public void PlthookClose(nint pltHook) => PinvokePlthookClose(pltHook);
    public nint PlthookError() => PinvokePlthookError();
}