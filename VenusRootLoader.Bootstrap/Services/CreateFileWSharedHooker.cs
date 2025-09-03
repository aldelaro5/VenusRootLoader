using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap.Services;

/// <summary>
/// This class allows the bootstrap to use a shared CreateFileW plt hook that many modules can use to listen for files
/// they are interested in. Each module can register a sub hook that only runs on files whose filename matches a predicate,
/// and they can decide to remove themselves from the hook list or change the handle returned
/// </summary>
internal class CreateFileWSharedHooker
{
    /// <summary>
    /// A sub hook to CreateFileW
    /// </summary>
    /// <param name="handle">The handle to return</param>
    /// <returns>True when the hook should be kept in the hook list or false if it should be removed upon return</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    internal delegate bool CreateFileWHook(
        out nint handle,
        string lpFileName,
        uint dwDesiredAccess,
        int dwShareMode,
        nint lpSecurityAttributes,
        int dwCreationDisposition,
        int dwFlagsAndAttributes,
        nint hTemplateFile);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private delegate nint CreateFileWFn(
        string lpFileName,
        uint dwDesiredAccess,
        int dwShareMode,
        nint lpSecurityAttributes,
        int dwCreationDisposition,
        int dwFlagsAndAttributes,
        nint hTemplateFile);
    private static CreateFileWFn _hookCreateFileWDelegate = null!;

    private readonly PltHook _pltHook;
    private readonly GameExecutionContext _gameExecutionContext;

    private readonly List<(Func<string, bool> predicate, CreateFileWHook Hook)> _fileHandlesHooks = new();

    public CreateFileWSharedHooker(PltHook pltHook, GameExecutionContext gameExecutionContext)
    {
        _pltHook = pltHook;
        _gameExecutionContext = gameExecutionContext;
        _hookCreateFileWDelegate = HookCreateFileW;
        _pltHook.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW", Marshal.GetFunctionPointerForDelegate(_hookCreateFileWDelegate));
    }

    /// <summary>
    /// Registers a CreateFileW sub hook
    /// </summary>
    /// <param name="predicate">A predicate for the filename that returns true if the hook should execute</param>
    /// <param name="hook">The CreateFileW sub hook, see the <see cref="CreateFileWHook"/> documentation to learn more</param>
    internal void RegisterHook(Func<string, bool> predicate, CreateFileWHook hook)
    {
        _fileHandlesHooks.Add((predicate, hook));
    }

    private nint HookCreateFileW(string lpFilename, uint dwDesiredAccess, int dwShareMode, nint lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, nint hTemplateFile)
    {
        for (var i = 0; i < _fileHandlesHooks.Count; i++)
        {
            var hookWithPredicate = _fileHandlesHooks[i];
            if (!hookWithPredicate.predicate(lpFilename))
                continue;

            var keepHook = hookWithPredicate.Hook(out var fileHandle, lpFilename, dwDesiredAccess, dwShareMode,
                lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            if (!keepHook)
                _fileHandlesHooks.RemoveAt(i);
            return fileHandle;
        }

        return WindowsNative.CreateFileW(lpFilename, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
    }
}
