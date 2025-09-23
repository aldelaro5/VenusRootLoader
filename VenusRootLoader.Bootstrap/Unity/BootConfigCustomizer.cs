using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Unity;

/// <summary>
/// <para>
/// This service implements a way to change the contents of boot.config read at runtime by UnityPlayer. The format and
/// documentations are in the boot.jsonc file in this repository. It is an internal Unity file provided when the game
/// is built.
/// </para>
/// <para>
/// This is an experimental feature. There's almost no useful need to modify the default boot.config, but this is done
/// just in case it happens to be handy.
/// </para>
/// </summary>
internal class BootConfigCustomizer : IHostedService
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private unsafe delegate int ReadFileFn(
        HANDLE hFile,
        byte* lpBuffer,
        uint nNumberOfBytesToRead,
        uint* lpNumberOfBytesRead,
        NativeOverlapped* lpOverlapped);
    private static ReadFileFn _hookReadFileDelegate = null!;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private unsafe delegate int SetFilePointerExFn(
        HANDLE hFile,
        long liDistanceToMove,
        long* lpNewFilePointer,
        SET_FILE_POINTER_MOVE_METHOD dwMoveMethod);
    private static SetFilePointerExFn _hookSetFilePointerDelegate = null!;

    private readonly IWin32 _win32;
    private readonly ILogger _logger;
    private readonly CreateFileWSharedHooker _createFileWSharedHooker;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly IPltHooksManager _pltHooksManager;
    private readonly BootConfigSettings _bootConfigSettings;
    private readonly GameLifecycleEvents _gameLifecycleEvents;
    private readonly string _bootConfigPath;
    private HANDLE _bootConfigFileHandle = HANDLE.Null;

    private readonly string _modifiedBootConfig;
    private readonly byte[] _bootConfigBytes;

    private long _modifiedFilePointer;

    public unsafe BootConfigCustomizer(
        ILogger<BootConfigCustomizer> logger,
        IPltHooksManager pltHooksManager,
        CreateFileWSharedHooker createFileWSharedHooker,
        GameExecutionContext gameExecutionContext,
        IOptions<BootConfigSettings> bootConfigSettings,
        GameLifecycleEvents gameLifecycleEvents,
        IWin32 win32)
    {
        _logger = logger;
        _pltHooksManager = pltHooksManager;
        _gameExecutionContext = gameExecutionContext;
        _gameLifecycleEvents = gameLifecycleEvents;
        _win32 = win32;
        _bootConfigSettings = bootConfigSettings.Value;
        _createFileWSharedHooker = createFileWSharedHooker;

        _modifiedBootConfig = BuildModifiedBootConfig();
        _bootConfigBytes = Encoding.UTF8.GetBytes(_modifiedBootConfig);

        _hookReadFileDelegate = ReadFileHook;
        _hookSetFilePointerDelegate = SetFilePointerEx;
        _bootConfigPath = Path.Combine(_gameExecutionContext.DataDir, "boot.config").Replace('\\', '/');
    }

    private string BuildModifiedBootConfig()
    {
        var sb = new StringBuilder();
        // These 5 keys must go first since that's how the vanilla boot.config is laid out.
        // gfx-enable-native-gfx-jobs by default is empty string which is the same as the key being absent, but we try to
        // replicate defaults as much as possible so we put empty string if null just for this one
        sb.Append($"gfx-enable-native-gfx-jobs={_bootConfigSettings.GfxEnableNativeGfxJobs.ToString()}\n");
        AppendKeyValuePair(sb, "wait-for-native-debugger", _bootConfigSettings.WaitForNativeDebugger);
        AppendKeyValuePair(sb, "scripting-runtime-version", _bootConfigSettings.ScriptingRuntimeVersion);
        AppendKeyValuePair(sb, "vr-enabled", _bootConfigSettings.VrEnabled);
        AppendKeyValuePair(sb, "hdr-display-enabled", _bootConfigSettings.HdrDisplayEnabled);

        AppendKeyValuePair(sb, "wait-for-managed-debugger", _bootConfigSettings.WaitForManagedDebugger);
        AppendKeyValuePair(sb, "mono-codegen", _bootConfigSettings.MonoCodeGen);
        AppendKeyValuePair(sb, "max-num-loops-no-job-before-going-idle", _bootConfigSettings.MaxNumLoopsNoJobBeforeGoingIdle);
        AppendKeyValuePair(sb, "preload-manager-thread-stack-size", _bootConfigSettings.PreloadManagerThreadStackSize);

        AppendKeyValuePair(sb, "force-gfx-direct", _bootConfigSettings.ForceGfxDirect);
        AppendKeyValuePair(sb, "force-gfx-st", _bootConfigSettings.ForceGfxSt);
        AppendKeyValuePair(sb, "force-gfx-mt", _bootConfigSettings.ForceGfxMt);
        AppendKeyValuePair(sb, "force-gfx-jobs", _bootConfigSettings.ForceGfxJobs);
        AppendKeyValuePair(sb, "gfx-enable-gfx-jobs", _bootConfigSettings.GfxEnableGfxJobs);
        AppendKeyValuePair(sb, "gfx-jobs-sync", _bootConfigSettings.GfxJobsSync);
        AppendKeyValuePair(sb, "gfx-disable-mt-rendering", _bootConfigSettings.GfxDisableMtRendering);

        AppendKeyValuePair(sb, "http-filesystem-enable", _bootConfigSettings.HttpFilesystemEnable);
        AppendKeyValuePair(sb, "http-filesystem-prefix", _bootConfigSettings.HttpFilesystemPrefix);
        AppendKeyValuePair(sb, "http-filesystem-apikey", _bootConfigSettings.HttpFilesystemApiKey);
        AppendKeyValuePair(sb, "http-filesystem-pubkey", _bootConfigSettings.HttpFilesystemPubKey);

        AppendKeyValuePair(sb, "player-connection-ip", _bootConfigSettings.PlayerConnectionIp);
        AppendKeyValuePair(sb, "player-connection-mode", _bootConfigSettings.PlayerConnectionMode);
        AppendKeyValuePair(sb, "player-connection-debug", _bootConfigSettings.PlayerConnectionDebug);
        AppendKeyValuePair(sb, "player-connection-guid", _bootConfigSettings.PlayerConnectionGuid);
        AppendKeyValuePair(sb, "player-connection-listen-address", _bootConfigSettings.PlayerConnectionListenAddress);
        AppendKeyValuePair(sb, "player-connection-wait-timeout", _bootConfigSettings.PlayerConnectionWaitTimeout);

        AppendKeyValuePair(sb, "profiler-maxpoolmemory", _bootConfigSettings.ProfilerMaxPoolMemory);
        AppendKeyValuePair(sb, "profiler-maxusedmemory", _bootConfigSettings.ProfilerMaxUsedMemory);
        AppendKeyValuePair(sb, "profiler-enable-on-startup", _bootConfigSettings.ProfilerEnableOnStartup);

        AppendKeyValuePair(sb, "headless", _bootConfigSettings.Headless);
        AppendKeyValuePair(sb, "single-instance", _bootConfigSettings.SingleInstance);

        return sb.ToString();
    }

    private static void AppendKeyValuePair(StringBuilder sb, string key, bool? value)
    {
        if (value is not null)
            sb.Append($"{key}={(value.Value ? "1" : "0")}\n");
    }

    private static void AppendKeyValuePair(StringBuilder sb, string key, int? value)
    {
        if (value is not null)
            sb.Append($"{key}={value}\n");
    }

    private static void AppendKeyValuePair(StringBuilder sb, string key, string? value)
    {
        if (value is not null)
            sb.Append($"{key}={value}\n");
    }

    public unsafe Task StartAsync(CancellationToken cancellationToken)
    {
        _createFileWSharedHooker.RegisterHook(nameof(BootConfigCustomizer), IsBootConfig, HookFileHandle);
        _pltHooksManager.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.ReadFile), Marshal.GetFunctionPointerForDelegate(_hookReadFileDelegate));
        _pltHooksManager.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.SetFilePointerEx), Marshal.GetFunctionPointerForDelegate(_hookSetFilePointerDelegate));
        _gameLifecycleEvents.Subscribe(OnGameLifecycle);
        _logger.LogDebug("The boot.config file will be modified to:\n{modifiedBootConfig}", _modifiedBootConfig);
        return Task.CompletedTask;
    }

    private void OnGameLifecycle(object? sender, GameLifecycleEventArgs e)
    {
        if (e.LifeCycle != GameLifecycle.MonoInitialising)
            return;
        _pltHooksManager.UninstallHook(_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.ReadFile));
        _pltHooksManager.UninstallHook(_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.SetFilePointerEx));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private bool IsBootConfig(string filename) => filename == _bootConfigPath;

    private unsafe void HookFileHandle(out HANDLE originalHandle, PCWSTR lpFileName, uint dwDesiredAccess, FILE_SHARE_MODE dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, FILE_CREATION_DISPOSITION dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, HANDLE hTemplateFile)
    {
        originalHandle = _win32.CreateFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
                dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        _bootConfigFileHandle = originalHandle;
        _logger.LogInformation("Opened boot.config with handle {_bootConfigFileHandle:X}", _bootConfigFileHandle);
        _createFileWSharedHooker.UnregisterHook(nameof(BootConfigCustomizer));
    }

    private unsafe int SetFilePointerEx(HANDLE hFile, long liDistanceToMove, long* lpNewFilePointer, SET_FILE_POINTER_MOVE_METHOD dwMoveMethod)
    {
        if (hFile == _bootConfigFileHandle)
        {
            switch (dwMoveMethod)
            {
                case SET_FILE_POINTER_MOVE_METHOD.FILE_BEGIN:
                    _modifiedFilePointer = (long)(0 + (ulong)liDistanceToMove);
                    break;
                case SET_FILE_POINTER_MOVE_METHOD.FILE_CURRENT:
                    _modifiedFilePointer += liDistanceToMove;
                    break;
                case SET_FILE_POINTER_MOVE_METHOD.FILE_END:
                    _modifiedFilePointer = _modifiedBootConfig.Length + liDistanceToMove;
                    break;
            }

            if (lpNewFilePointer != null)
                *lpNewFilePointer = _modifiedFilePointer;

            _logger.LogInformation("Set boot.config file pointer to {ptr}", _modifiedFilePointer);
            return 1;
        }

        return _win32.SetFilePointerEx(hFile, liDistanceToMove, lpNewFilePointer, dwMoveMethod);
    }

    private unsafe int ReadFileHook(HANDLE hFile, byte* lpBuffer, uint nNumberOfBytesToRead, uint* lpNumberOfBytesRead, NativeOverlapped* lpOverlapped)
    {
        if (hFile == _bootConfigFileHandle)
        {
            var cappedNumberBytesToRead = _modifiedFilePointer + nNumberOfBytesToRead > _modifiedBootConfig.Length
                ? _modifiedBootConfig.Length - _modifiedFilePointer
                : nNumberOfBytesToRead;
            Marshal.Copy(_bootConfigBytes, (int)_modifiedFilePointer, (nint)lpBuffer, (int)cappedNumberBytesToRead);
            *lpNumberOfBytesRead = (uint)cappedNumberBytesToRead;
            _bootConfigFileHandle = HANDLE.Null;
            _logger.LogInformation("Read {nbrBytes} bytes", cappedNumberBytesToRead);
            return 1;
        }

        return _win32.ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, lpNumberOfBytesRead, lpOverlapped);
    }
}