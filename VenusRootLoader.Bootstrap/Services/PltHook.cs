using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class contains PInvoke abstractions for the PltHook library that's statically linked in the bootstrap
/// </summary>
internal partial class PltHook
{
    [LibraryImport("*", EntryPoint = "plthook_open", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int PlthookOpen(ref nint pltHookOut, string? filename);

    [LibraryImport("*", EntryPoint = "plthook_replace", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int PlthookReplace(nint pltHook, string funcName, nint funcAddr, nint oldFunc);

    [LibraryImport("*", EntryPoint = "plthook_close")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void PlthookClose(nint pltHook);

    [LibraryImport("*", EntryPoint = "plthook_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint PlthookError();

    private readonly Dictionary<string, nint> OpenedPltHooksByFilename = new();

    private readonly ILogger _logger;

    public PltHook(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(nameof(PltHook), Color.Magenta);
    }

    internal void InstallHook(string fileName, string functionName, nint hookFunctionPtr)
    {
        if (!OpenedPltHooksByFilename.TryGetValue(fileName, out var pltHookPtr))
        {
            nint pltHook = IntPtr.Zero;
            var pltHookOpened = PlthookOpen(ref pltHook, fileName) == 0;

            if (!pltHookOpened)
            {
                _logger.LogError($"plthook_open error: {Marshal.PtrToStringAuto(PlthookError())}");
                return;
            }

            OpenedPltHooksByFilename.Add(fileName, pltHook);
            pltHookPtr = pltHook;
            _logger.LogInformation($"plthook_open: Opened with filename {fileName} successfully");
        }

        if (PlthookReplace(pltHookPtr, functionName, hookFunctionPtr, IntPtr.Zero) != 0)
        {
            _logger.LogError($"plthook_replace error: when hooking {functionName}: {Marshal.PtrToStringUTF8(PlthookError())}");
            return;
        }

        _logger.LogInformation($"plthook_replace: Plt hooked {functionName} successfully");
    }

    internal void CloseFilenameHooksHandleIfExists(string fileName)
    {
        if (!OpenedPltHooksByFilename.TryGetValue(fileName, out var pltHookPtr))
            return;
        PlthookClose(pltHookPtr);
        OpenedPltHooksByFilename.Remove(fileName);
        _logger.LogInformation($"plthook_close: Closed with filename {fileName}");
    }
}