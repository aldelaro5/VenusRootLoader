using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using PltHook = VenusRootLoader.Bootstrap.Shared.PltHook;

namespace VenusRootLoader.Bootstrap.Unity;

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

    private readonly ILogger _logger;
    private readonly CreateFileWSharedHooker _createFileWSharedHooker;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly PltHook _pltHook;
    private readonly BootConfigSettings _bootConfigSettings;
    private readonly string _bootConfigPath;
    private HANDLE _bootConfigFileHandle = HANDLE.Null;

    // gfx-enable-native-gfx-jobs=
    // wait-for-native-debugger=0
    // scripting-runtime-version=latest
    // vr-enabled=0
    // hdr-display-enabled=0

    // mono-codegen=il2cpp
    // max-num-loops-no-job-before-going-idle=10
    // wait-for-managed-debugger=0
    // preload-manager-thread-stack-size=0
    // gfx-disable-mt-rendering=0
    // gfx-enable-gfx-jobs=0
    // force-gfx-direct=0
    // force-gfx-st=0
    // force-gfx-mt=0
    // force-gfx-jobs=0
    // gfx-jobs-sync=0
    // http-filesystem-apikey=
    // http-filesystem-enable=0
    // http-filesystem-prefix=
    // http-filesystem-pubkey=
    // --headless
    // --single-instance

    // player-connection-ip=0
    // player-connection-debug=0
    // player-connection-mode=0
    // player-connection-guid=0
    // player-connection-listen-address=0.0.0.0
    // player-connection-wait-timeout=-1
    // profiler-maxpoolmemory=4194304 (0x400000)
    // profiler-maxusedmemory=16777216 (0x1000000)
    // profiler-enable-on-startup=0

    private readonly string _modifiedBootConfig;
    private readonly byte[] _bootConfigBytes;

    private long _modifiedFilePointer;

    public unsafe BootConfigCustomizer(
        ILogger<BootConfigCustomizer> logger,
        PltHook pltHook,
        CreateFileWSharedHooker createFileWSharedHooker,
        GameExecutionContext gameExecutionContext,
        IOptions<BootConfigSettings> bootConfigSettings)
    {
        _logger = logger;
        _pltHook = pltHook;
        _gameExecutionContext = gameExecutionContext;
        _bootConfigSettings = bootConfigSettings.Value;
        _createFileWSharedHooker = createFileWSharedHooker;

        _modifiedBootConfig = BuildModifiedBootConfig();
        _bootConfigBytes = Encoding.UTF8.GetBytes(_modifiedBootConfig);

        _hookReadFileDelegate = ReadFileHook;
        _hookSetFilePointerDelegate = SetFilePointerEx;
        _bootConfigPath = Path.Combine(_gameExecutionContext.DataDir, "boot.config").Replace('\\', '/');
        _logger.LogDebug("The boot.config file will be modified to:\n{modifiedBootConfig}", _modifiedBootConfig);
    }

    private string BuildModifiedBootConfig()
    {
        var sb = new StringBuilder();
        sb.Append($"gfx-enable-native-gfx-jobs={_bootConfigSettings.GfxEnableNativeGfxJobs.ToString()}\n");
        if (_bootConfigSettings.WaitForNativeDebugger is not null)
            sb.Append($"wait-for-native-debugger={_bootConfigSettings.WaitForNativeDebugger}\n");
        if (_bootConfigSettings.ScriptingRuntimeVersion is not null)
            sb.Append($"scripting-runtime-version={_bootConfigSettings.ScriptingRuntimeVersion}\n");
        if (_bootConfigSettings.VrEnabled is not null)
            sb.Append($"vr-enabled={_bootConfigSettings.VrEnabled}\n");
        if (_bootConfigSettings.HdrDisplayEnabled is not null)
            sb.Append($"hdr-display-enabled={_bootConfigSettings.HdrDisplayEnabled}\n");
        if (_bootConfigSettings.MaxNumLoopsNoJobBeforeGoingIdle is not null)
            sb.Append($"max-num-loops-no-job-before-going-idle={_bootConfigSettings.MaxNumLoopsNoJobBeforeGoingIdle}\n");
        if (_bootConfigSettings.WaitForManagedDebugger is not null)
            sb.Append($"wait-for-managed-debugger={_bootConfigSettings.WaitForManagedDebugger}\n");
        if (_bootConfigSettings.PreloadManagerThreadStackSize is not null)
            sb.Append($"preload-manager-thread-stack-size={_bootConfigSettings.PreloadManagerThreadStackSize}\n");
        if (_bootConfigSettings.GfxDisableMtRendering is not null)
            sb.Append($"gfx-disable-mt-rendering={_bootConfigSettings.GfxDisableMtRendering}\n");
        if (_bootConfigSettings.GfxEnableGfxJobs is not null)
            sb.Append($"gfx-enable-gfx-jobs={_bootConfigSettings.GfxEnableGfxJobs}\n");
        if (_bootConfigSettings.ForceGfxDirect is not null)
            sb.Append($"force-gfx-direct={_bootConfigSettings.ForceGfxDirect}\n");
        if (_bootConfigSettings.ForceGfxSt is not null)
            sb.Append($"force-gfx-st={_bootConfigSettings.ForceGfxSt}\n");
        if (_bootConfigSettings.ForceGfxMt is not null)
            sb.Append($"force-gfx-mt={_bootConfigSettings.ForceGfxMt}\n");
        if (_bootConfigSettings.ForceGfxJobs is not null)
            sb.Append($"force-gfx-jobs={_bootConfigSettings.ForceGfxJobs}\n");
        if (_bootConfigSettings.GfxJobsSync is not null)
            sb.Append($"gfx-jobs-sync={_bootConfigSettings.GfxJobsSync}\n");
        if (_bootConfigSettings.HttpFilesystemApiKey is not null)
            sb.Append($"http-filesystem-apikey={_bootConfigSettings.HttpFilesystemApiKey}\n");
        if (_bootConfigSettings.HttpFilesystemEnable is not null)
            sb.Append($"http-filesystem-enable={_bootConfigSettings.HttpFilesystemEnable}\n");
        if (_bootConfigSettings.HttpFilesystemPrefix is not null)
            sb.Append($"http-filesystem-prefix={_bootConfigSettings.HttpFilesystemPrefix}\n");
        if (_bootConfigSettings.HttpFilesystemPubKey is not null)
            sb.Append($"http-filesystem-pubkey={_bootConfigSettings.HttpFilesystemPubKey}\n");
        if (_bootConfigSettings.PlayerConnectionIp is not null)
            sb.Append($"player-connection-ip={_bootConfigSettings.PlayerConnectionIp}\n");
        if (_bootConfigSettings.PlayerConnectionDebug is not null)
            sb.Append($"player-connection-debug={_bootConfigSettings.PlayerConnectionDebug}\n");
        if (_bootConfigSettings.PlayerConnectionMode is not null)
            sb.Append($"player-connection-mode={_bootConfigSettings.PlayerConnectionMode}\n");
        if (_bootConfigSettings.PlayerConnectionGuid is not null)
            sb.Append($"player-connection-guid={_bootConfigSettings.PlayerConnectionGuid}\n");
        if (_bootConfigSettings.PlayerConnectionListenAddress is not null)
            sb.Append($"player-connection-listen-address={_bootConfigSettings.PlayerConnectionListenAddress}\n");
        if (_bootConfigSettings.PlayerConnectionWaitTimeout is not null)
            sb.Append($"player-connection-wait-timeout={_bootConfigSettings.PlayerConnectionWaitTimeout}\n");
        if (_bootConfigSettings.ProfilerMaxPoolMemory is not null)
            sb.Append($"profiler-maxpoolmemory={_bootConfigSettings.ProfilerMaxPoolMemory})\n");
        if (_bootConfigSettings.ProfilerMaxUsedMemory is not null)
            sb.Append($"profiler-maxusedmemory={_bootConfigSettings.ProfilerMaxUsedMemory}\n");
        if (_bootConfigSettings.ProfilerEnableOnStartup is not null)
            sb.Append($"profiler-enable-on-startup={_bootConfigSettings.ProfilerEnableOnStartup}");
        if (_bootConfigSettings.Headless is not null)
            sb.Append($"headless={_bootConfigSettings.Headless}");
        if (_bootConfigSettings.SingleInstance is not null)
            sb.Append($"single-instance={_bootConfigSettings.SingleInstance}");

        return sb.ToString();
    }

    public unsafe Task StartAsync(CancellationToken cancellationToken)
    {
        _createFileWSharedHooker.RegisterHook(IsBootConfig, HookFileHandle);
        _pltHook.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, nameof(PInvoke.ReadFile), Marshal.GetFunctionPointerForDelegate(_hookReadFileDelegate));
        _pltHook.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, nameof(PInvoke.SetFilePointerEx), Marshal.GetFunctionPointerForDelegate(_hookSetFilePointerDelegate));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private bool IsBootConfig(string filename) => filename == _bootConfigPath;

    private unsafe bool HookFileHandle(out HANDLE originalHandle, PCWSTR lpFileName, uint dwDesiredAccess, FILE_SHARE_MODE dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, FILE_CREATION_DISPOSITION dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, HANDLE hTemplateFile)
    {
        originalHandle = PInvoke.CreateFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
                dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        _bootConfigFileHandle = originalHandle;
        _logger.LogInformation("Opened boot.config with handle {_bootConfigFileHandle:X}", _bootConfigFileHandle);
        return false;
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

        return PInvoke.SetFilePointerEx(hFile, liDistanceToMove, lpNewFilePointer, dwMoveMethod);
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

        return PInvoke.ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, lpNumberOfBytesRead, lpOverlapped);
    }
}