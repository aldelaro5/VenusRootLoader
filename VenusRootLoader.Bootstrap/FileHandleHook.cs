using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap;

internal static class FileHandleHook
{
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
    private static readonly CreateFileWFn HookCreateFileWDelegate = HookCreateFileW;

    private static readonly List<(Func<string, bool> predicate, CreateFileWHook Hook)> FileHandlesHooks = new();

    internal static void Setup()
    {
        PltHook.InstallHook(Entry.UnityPlayerDllFileName, "CreateFileW", Marshal.GetFunctionPointerForDelegate(HookCreateFileWDelegate));
    }

    internal static void RegisterHook(Func<string, bool> predicate, CreateFileWHook hook)
    {
        FileHandlesHooks.Add((predicate, hook));
    }

    private static nint HookCreateFileW(string lpFilename, uint dwDesiredAccess, int dwShareMode, nint lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, nint hTemplateFile)
    {
        for (var i = 0; i < FileHandlesHooks.Count; i++)
        {
            var hookWithPredicate = FileHandlesHooks[i];
            if (!hookWithPredicate.predicate(lpFilename))
                continue;

            var keepHook = hookWithPredicate.Hook(out var fileHandle, lpFilename, dwDesiredAccess, dwShareMode,
                lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            if (!keepHook)
                FileHandlesHooks.RemoveAt(i);
            return fileHandle;
        }

        return WindowsNative.CreateFileW(lpFilename, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
    }
}
