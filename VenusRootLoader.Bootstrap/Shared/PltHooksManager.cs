using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap.Shared;

using ModulePltHook = (nint ptr, Dictionary<string, nint> originalHookedFunc);

public interface IPltHooksManager
{
    void InstallHook<T>(string fileName, string functionName, T hook) where T : Delegate;
    void UninstallHook(string fileName, string functionName);
}

/// <summary>
/// This class contains PInvoke abstractions for the PltHook library that's statically linked in the bootstrap
/// </summary>
public class PltHooksManager : IPltHooksManager
{
    private readonly IFileSystem _fileSystem;
    private readonly Dictionary<string, ModulePltHook> _openedPltHooksByFilename = new();

    private readonly ILogger _logger;
    private readonly IPltHook _pltHook;

    public PltHooksManager(
        ILogger<PltHooksManager> logger,
        IPltHook pltHook,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _pltHook = pltHook;
        _fileSystem = fileSystem;
    }

    public unsafe void InstallHook<T>(string fileName, string functionName, T hook) where T : Delegate
    {
        if (!_openedPltHooksByFilename.TryGetValue(fileName, out var moduleHook))
        {
            ModulePltHook newModuleHook = (nint.Zero, new Dictionary<string, nint>());
            if (!_pltHook.PlthookOpen(new(&newModuleHook.ptr), fileName))
            {
                _logger.LogError($"plthook_open error: {Marshal.PtrToStringUTF8(_pltHook.PlthookError())}");
                return;
            }

            _openedPltHooksByFilename.Add(fileName, newModuleHook);
            moduleHook = newModuleHook;
            _logger.LogInformation($"plthook_open: Opened with filename {fileName} successfully");
        }

        nint addressOriginal = nint.Zero;
        if (!_pltHook.PlthookReplace(moduleHook.ptr, functionName, Marshal.GetFunctionPointerForDelegate(hook), new(&addressOriginal)))
        {
            _logger.LogError($"plthook_replace error: when hooking {functionName}: {Marshal.PtrToStringUTF8(_pltHook.PlthookError())}");
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

        var oldFunc = nint.Zero;
        if (!_pltHook.PlthookReplace(moduleHook.ptr, functionName, originalHookedFunc, new(&oldFunc)))
        {
            _logger.LogError($"plthook_replace error: when unhooking {functionName}: {Marshal.PtrToStringUTF8(_pltHook.PlthookError())}");
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

        _pltHook.PlthookClose(moduleHook.ptr);
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
            _logger.LogTrace("\t{fileName}", _fileSystem.Path.GetFileName(moduleHook.Key));
            foreach (var functionHook in moduleHook.Value.originalHookedFunc.Keys)
                _logger.LogTrace("\t\t{functionName}", functionHook);
        }
    }
}