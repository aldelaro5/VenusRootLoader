using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;

namespace VenusRootLoader.Bootstrap.Shared;

public interface ICreateFileWSharedHooker
{
    /// <summary>
    /// Registers a CreateFileW sub hook
    /// </summary>
    /// <param name="name">The name of the hook</param>
    /// <param name="predicate">A predicate for the filename that returns true if the hook should execute</param>
    /// <param name="hook">The CreateFileW sub hook, see the <see cref="CreateFileWSharedHooker.CreateFileWHook"/> documentation to learn more</param>
    void RegisterHook(string name, Func<string, bool> predicate, CreateFileWSharedHooker.CreateFileWHook hook);

    /// <summary>
    /// Unregisters a CreateFileW sub hook
    /// </summary>
    /// <param name="name">The name of the hook to unregister</param>
    void UnregisterHook(string name);
}

/// <summary>
/// This class allows the bootstrap to use a shared CreateFileW plt hook that many modules can use to listen for files
/// they are interested in. Each module can register a sub hook that only runs on files whose filename matches a predicate,
/// and they can decide to remove themselves from the hook list or change the handle returned
/// </summary>
public class CreateFileWSharedHooker : ICreateFileWSharedHooker
{
    /// <summary>
    /// A sub hook to CreateFileW
    /// </summary>
    /// <param name="handle">The handle to return</param>
    /// <returns>True when the hook should be kept in the hook list or false if it should be removed upon return</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public unsafe delegate void CreateFileWHook(
        out HANDLE handle,
        PCWSTR lpFileName,
        uint dwDesiredAccess,
        FILE_SHARE_MODE dwShareMode,
        SECURITY_ATTRIBUTES* lpSecurityAttributes,
        FILE_CREATION_DISPOSITION dwCreationDisposition,
        FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes,
        HANDLE hTemplateFile);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private unsafe delegate nint CreateFileWFn(
        PCWSTR lpFileName,
        uint dwDesiredAccess,
        FILE_SHARE_MODE dwShareMode,
        SECURITY_ATTRIBUTES* lpSecurityAttributes,
        FILE_CREATION_DISPOSITION dwCreationDisposition,
        FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes,
        HANDLE hTemplateFile);
    private static CreateFileWFn _hookCreateFileWDelegate = null!;

    private readonly IWin32 _win32;
    private readonly IPltHooksManager _pltHooksManager;
    private readonly GameExecutionContext _gameExecutionContext;

    private readonly Dictionary<string, (Func<string, bool> predicate, CreateFileWHook Hook)> _fileHandlesHooks = new();

    public unsafe CreateFileWSharedHooker(
        IPltHooksManager pltHooksManager,
        GameExecutionContext gameExecutionContext,
        IWin32 win32)
    {
        _pltHooksManager = pltHooksManager;
        _gameExecutionContext = gameExecutionContext;
        _win32 = win32;
        _hookCreateFileWDelegate = HookCreateFileW;
        _pltHooksManager.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW", _hookCreateFileWDelegate);
    }

    /// <summary>
    /// Registers a CreateFileW sub hook
    /// </summary>
    /// <param name="name">The name of the hook</param>
    /// <param name="predicate">A predicate for the filename that returns true if the hook should execute</param>
    /// <param name="hook">The CreateFileW sub hook, see the <see cref="CreateFileWHook"/> documentation to learn more</param>
    public void RegisterHook(string name, Func<string, bool> predicate, CreateFileWHook hook)
    {
        _fileHandlesHooks.Add(name, (predicate, hook));
    }

    /// <summary>
    /// Unregisters a CreateFileW sub hook
    /// </summary>
    /// <param name="name">The name of the hook to unregister</param>
    public void UnregisterHook(string name)
    {
        _fileHandlesHooks.Remove(name);
        if (_fileHandlesHooks.Count <= 0)
            _pltHooksManager.UninstallHook(_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW");
    }

    public unsafe nint HookCreateFileW(PCWSTR lpFileName, uint dwDesiredAccess, FILE_SHARE_MODE dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, FILE_CREATION_DISPOSITION dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, HANDLE hTemplateFile)
    {
        foreach (var hookWithPredicate in _fileHandlesHooks.Values)
        {
            if (!hookWithPredicate.predicate(lpFileName.ToString()))
                continue;

            hookWithPredicate.Hook(out var fileHandle, lpFileName, dwDesiredAccess, dwShareMode,
                lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

            return fileHandle;
        }
        return _win32.CreateFile(lpFileName, dwDesiredAccess, dwShareMode, new(lpSecurityAttributes),
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
    }
}