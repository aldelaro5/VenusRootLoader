using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Bootstrap.Shared;

using ModulePltHook = (nint ptr, Dictionary<string, nint> originalHookedFunc);

/// <summary>
/// This class contains PInvoke abstractions for the PltHook library that's statically linked in the bootstrap
/// </summary>
public partial class PltHook
{
    [LibraryImport("*", EntryPoint = "plthook_open", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int PlthookOpen(ref nint pltHookOut, string? filename);

    [LibraryImport("*", EntryPoint = "plthook_replace", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int PlthookReplace(nint pltHook, string funcName, nint funcAddr, nint* oldFunc);

    [LibraryImport("*", EntryPoint = "plthook_close")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void PlthookClose(nint pltHook);

    [LibraryImport("*", EntryPoint = "plthook_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint PlthookError();

    private readonly Dictionary<string, ModulePltHook> _openedPltHooksByFilename = new();

    private readonly ILogger _logger;

    public PltHook(ILogger<PltHook> logger)
    {
        _logger = logger;
    }

    internal unsafe void InstallHook(string fileName, string functionName, nint hookFunctionPtr)
    {
        if (!_openedPltHooksByFilename.TryGetValue(fileName, out var moduleHook))
        {
            ModulePltHook newModuleHook = (nint.Zero, new Dictionary<string, nint>());
            var pltHookOpened = PlthookOpen(ref newModuleHook.ptr, fileName) == 0;

            if (!pltHookOpened)
            {
                _logger.LogError($"plthook_open error: {Marshal.PtrToStringUTF8(PlthookError())}");
                return;
            }

            _openedPltHooksByFilename.Add(fileName, newModuleHook);
            moduleHook = newModuleHook;
            _logger.LogInformation($"plthook_open: Opened with filename {fileName} successfully");
        }

        nint addressOriginal = nint.Zero;
        if (PlthookReplace(moduleHook.ptr, functionName, hookFunctionPtr, &addressOriginal) != 0)
        {
            _logger.LogError($"plthook_replace error: when hooking {functionName}: {Marshal.PtrToStringUTF8(PlthookError())}");
            return;
        }
        moduleHook.originalHookedFunc[functionName] = addressOriginal;
        _logger.LogInformation($"plthook_replace: Plt hooked {functionName} successfully");

        if (_logger.IsEnabled(LogLevel.Trace))
            LogAllActiveHooks();
    }

    public unsafe void UninstallHook(string fileName, string functionName)
    {
        if (!_openedPltHooksByFilename.TryGetValue(fileName, out var moduleHook))
            return;

        if (!moduleHook.originalHookedFunc.TryGetValue(functionName, out var originalHookedFunc))
            return;

        if (PlthookReplace(moduleHook.ptr, functionName, originalHookedFunc, null) != 0)
        {
            _logger.LogError($"plthook_replace error: when unhooking {functionName}: {Marshal.PtrToStringUTF8(PlthookError())}");
            return;
        }
        moduleHook.originalHookedFunc.Remove(functionName);
        _logger.LogInformation("Uninstalled hook with filename {FileName} and function {FunctionName} successfully",
            fileName, functionName);

        if (moduleHook.originalHookedFunc.Count > 0)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                LogAllActiveHooks();
            return;
        }

        PlthookClose(moduleHook.ptr);
        _logger.LogInformation($"plthook_close: Closed with filename {fileName}");
        _openedPltHooksByFilename.Remove(fileName);
        
        if (_logger.IsEnabled(LogLevel.Trace))
            LogAllActiveHooks();
    }

    private void LogAllActiveHooks()
    {
        _logger.LogTrace("All active hooks:");
        foreach (var moduleHook in _openedPltHooksByFilename)
        {
            _logger.LogTrace("\t{fileName}", Path.GetFileName(moduleHook.Key));
            foreach (var functionHook in moduleHook.Value.originalHookedFunc.Keys)
                _logger.LogTrace("\t\t{functionName}", functionHook);
        }
    }
}