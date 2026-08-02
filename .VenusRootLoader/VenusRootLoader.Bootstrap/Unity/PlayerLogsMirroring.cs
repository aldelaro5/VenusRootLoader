using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;

namespace VenusRootLoader.Bootstrap.Unity;

/// <summary>
/// This service contains all the machinery needed to fully capture and mirror stdout, stderr and Unity's player logs
/// into our logs
/// </summary>
internal sealed class PlayerLogsMirroring
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private unsafe delegate int WriteFileFn(
        HANDLE hFile,
        byte* lpBuffer,
        uint nNumberOfBytesToWrite,
        uint* lpNumberOfBytesWritten,
        NativeOverlapped* lpOverlapped);

    private static WriteFileFn _hookWriteFileDelegate = null!;

    private HANDLE _outputHandle;
    private HANDLE _errorHandle;
    private HANDLE _playerLogHandle = (HANDLE)nint.Zero;

    private readonly IWin32 _win32;
    private readonly IPltHooksManager _pltHooksManager;
    private readonly ILogger _logger;
    private readonly ICreateFileWSharedHooker _createFileWSharedHooker;
    private readonly IGameExecutionContext _gameExecutionContext;
    private readonly IMonoInitLifeCycleEvents _monoInitLifeCycleEvents;

    public unsafe PlayerLogsMirroring(
        ILoggerFactory loggerFactory,
        IPltHooksManager pltHooksManager,
        ICreateFileWSharedHooker createFileWSharedHooker,
        IGameExecutionContext gameExecutionContext,
        IMonoInitLifeCycleEvents monoInitLifeCycleEvents,
        IWin32 win32)
    {
        _pltHooksManager = pltHooksManager;
        _logger = loggerFactory.CreateLogger("UNITY");
        _createFileWSharedHooker = createFileWSharedHooker;
        _gameExecutionContext = gameExecutionContext;
        _monoInitLifeCycleEvents = monoInitLifeCycleEvents;
        _win32 = win32;
        _hookWriteFileDelegate = HookWriteFile;
    }

    public unsafe void MirrorLogs()
    {
        _outputHandle = _win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        _errorHandle = _win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);

        _pltHooksManager.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, "WriteFile", _hookWriteFileDelegate);
        _createFileWSharedHooker.RegisterHook(nameof(PlayerLogsMirroring), IsUnityPlayerLogFilename, HookFileHandle);
        _monoInitLifeCycleEvents.Subscribe(OnGameLifecycle);
    }

    private void OnGameLifecycle(object? sender, EventArgs e)
    {
        _createFileWSharedHooker.UnregisterHook(nameof(PlayerLogsMirroring));
    }

    private static bool IsUnityPlayerLogFilename(string lpFilename) =>
        lpFilename.EndsWith("Player.log") || lpFilename.EndsWith("output_log.txt");

    private unsafe void HookFileHandle(
        out HANDLE originalHandle,
        PCWSTR lpFileName,
        uint dwDesiredAccess,
        FILE_SHARE_MODE dwShareMode,
        SECURITY_ATTRIBUTES* lpSecurityAttributes,
        FILE_CREATION_DISPOSITION dwCreationDisposition,
        FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes,
        HANDLE hTemplateFile)
    {
        originalHandle = _win32.CreateFile(
            lpFileName,
            dwDesiredAccess,
            dwShareMode,
            new(lpSecurityAttributes),
            dwCreationDisposition,
            dwFlagsAndAttributes,
            hTemplateFile);
        _playerLogHandle = originalHandle;
        _createFileWSharedHooker.UnregisterHook(nameof(PlayerLogsMirroring));
    }

    // This hook is what collects every stdout, stderr or player logs done by Unity and writes them to our logs
    private unsafe int HookWriteFile(
        HANDLE hFile,
        byte* lpBuffer,
        uint nNumberOfBytesToWrite,
        uint* lpNumberOfBytesWritten,
        NativeOverlapped* lpOverlapped)
    {
        BOOL writeToPlayerLog = _win32.CompareObjectHandles(_playerLogHandle, hFile);
        bool writeToStandardHandles = _win32.CompareObjectHandles(hFile, _outputHandle) ||
                                      _win32.CompareObjectHandles(hFile, _errorHandle);
        if (!writeToPlayerLog && !writeToStandardHandles)
        {
            return _win32.WriteFile(
                hFile,
                new(lpBuffer),
                nNumberOfBytesToWrite,
                new(lpNumberOfBytesWritten),
                new(lpOverlapped));
        }

        if (!_logger.IsEnabled(LogLevel.Trace))
        {
            if (writeToStandardHandles)
                return 1;
            return _win32.WriteFile(
                hFile,
                new(lpBuffer),
                nNumberOfBytesToWrite,
                new(lpNumberOfBytesWritten),
                new(lpOverlapped));
        }

        string log = Marshal.PtrToStringUTF8((nint)lpBuffer, (int)nNumberOfBytesToWrite);
        _logger.LogTrace(log.TrimEnd("\r\n").ToString());

        if (writeToStandardHandles)
            return 1;

        return _win32.WriteFile(
            hFile,
            new(lpBuffer),
            nNumberOfBytesToWrite,
            new(lpNumberOfBytesWritten),
            new(lpOverlapped));
    }
}