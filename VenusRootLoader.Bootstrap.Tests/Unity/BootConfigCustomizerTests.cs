using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;
using VenusRootLoader.Bootstrap.Unity;

namespace VenusRootLoader.Bootstrap.Tests.Unity;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class BootConfigCustomizerTests
{
    private readonly ILogger<BootConfigCustomizer> _logger = Substitute.For<ILogger<BootConfigCustomizer>>();
    private readonly TestPltHookManager _pltHooksManager = new();
    private readonly CreateFileWSharedHooker _createFileWSharedHooker;
    private readonly IOptions<BootConfigSettings> _bootConfigSettings =  Substitute.For<IOptions<BootConfigSettings>>();
    private readonly TestGameLifecycleEvents _gameLifecycleEvents = new();
    private readonly IWin32 _win32 = Substitute.For<IWin32>();
    private readonly MockFileSystem _fileSystem = new();
    private readonly GameExecutionContext _gameExecutionContext = new()
    {
        LibraryHandle = 0,
        GameDir = "",
        DataDir = "Data",
        UnityPlayerDllFileName = "UnityPlayer.dll",
        IsWine = false
    };
    private readonly BootConfigSettings _bootConfigSettingsValue;
    private readonly string _bootConfigFilePath;

    public BootConfigCustomizerTests()
    {
        _createFileWSharedHooker = new(_pltHooksManager, _gameExecutionContext, _win32);
        _bootConfigSettingsValue = new();
        _bootConfigSettings.Value.Returns(_bootConfigSettingsValue);
        _bootConfigFilePath = Path.Combine(_gameExecutionContext.DataDir, "boot.config");
    }

    private void StartService()
    {
        var sut = new BootConfigCustomizer(
            _logger,
            _pltHooksManager,
            _createFileWSharedHooker,
            _gameExecutionContext,
            _bootConfigSettings,
            _gameLifecycleEvents,
            _win32,
            _fileSystem);
        sut.StartAsync(TestContext.Current.CancellationToken);
    }

    private bool GetRandomBool() => Random.Shared.Next() % 2 == 0;
    private int GetRandomInt() => Random.Shared.Next();

    [Fact]
    public unsafe void CreateFileHook_CallsOriginalAndOnlyKeepNeededHooks_WhenCreateFileIsCalledWithBootConfig()
    {
        StartService();

        var expectedReturn = (HANDLE)Random.Shared.Next();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(_bootConfigFilePath);
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(expectedReturn);

        var result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "CreateFileW",
            fileNamePtr,
            0u,
            default(FILE_SHARE_MODE),
            null,
            default(FILE_CREATION_DISPOSITION),
            default(FILE_FLAGS_AND_ATTRIBUTES),
            default(HANDLE))!;

        result.Should().Be(expectedReturn);
        _win32.ReceivedWithAnyArgs(1).CreateFile(fileNamePtr, 0, default, default, default, default, default);
        _pltHooksManager.Hooks.Should().NotContainKey((_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW"));
        _pltHooksManager.Hooks.Should().ContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.ReadFile)));
        _pltHooksManager.Hooks.Should().ContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.SetFilePointerEx)));
    }

    [Fact]
    public unsafe void SetFilePointerExHook_CallsOriginal_WhenHandleIsNotBootConfig()
    {
        StartService();

        var expectedReturn = Random.Shared.Next() % 2 == 0;
        var receivedHandle = (HANDLE)Random.Shared.Next();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(_bootConfigFilePath);
        _win32.SetFilePointerEx(
                Arg.Any<HANDLE>(),
                Arg.Any<long>(),
                Arg.Any<Pointer<long>>(),
                Arg.Any<SET_FILE_POINTER_MOVE_METHOD>())
            .ReturnsForAnyArgs((BOOL)expectedReturn);
        _pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "CreateFileW",
            fileNamePtr,
            0u,
            default(FILE_SHARE_MODE),
            null,
            default(FILE_CREATION_DISPOSITION),
            default(FILE_FLAGS_AND_ATTRIBUTES),
            default(HANDLE));

        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.SetFilePointerEx),
            receivedHandle,
            0,
            null,
            default(SET_FILE_POINTER_MOVE_METHOD))!;

        result.Should().Be((BOOL)expectedReturn);
        _win32.Received(1).SetFilePointerEx(receivedHandle, 0, default, default);
    }

    [Fact]
    public unsafe void SetFilePointerExHook_ReturnsModifiedPointer_WhenHandleIsBootConfig()
    {
        var settingValue = GetRandomBool();
        _bootConfigSettingsValue.Headless = settingValue;
        StartService();

        var expectedReturn = Random.Shared.Next() % 2 == 0;
        var receivedHandle = (HANDLE)Random.Shared.Next();
        var modifiedFilePointer = (long)0;
        var distanceToMove = Random.Shared.NextInt64(int.MinValue, int.MaxValue);
        var fileLength =
            "gfx-enable-native-gfx-jobs=\n".Length +
            nameof(_bootConfigSettingsValue.Headless).Length +
            3;
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(_bootConfigFilePath);
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(receivedHandle);
        _win32.SetFilePointerEx(
                Arg.Any<HANDLE>(),
                Arg.Any<long>(),
                Arg.Any<Pointer<long>>(),
                Arg.Any<SET_FILE_POINTER_MOVE_METHOD>())
            .ReturnsForAnyArgs((BOOL)expectedReturn);
        _pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "CreateFileW",
            fileNamePtr,
            0u,
            default(FILE_SHARE_MODE),
            null,
            default(FILE_CREATION_DISPOSITION),
            default(FILE_FLAGS_AND_ATTRIBUTES),
            default(HANDLE));

        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.SetFilePointerEx),
            receivedHandle,
            distanceToMove,
            (nint)(&modifiedFilePointer),
            SET_FILE_POINTER_MOVE_METHOD.FILE_BEGIN)!;

        result.Should().Be(1);
        modifiedFilePointer.Should().Be(0 + distanceToMove);

        var filePointer = modifiedFilePointer;
        distanceToMove = Random.Shared.NextInt64(int.MinValue, int.MaxValue);
        result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.SetFilePointerEx),
            receivedHandle,
            distanceToMove,
            (nint)(&modifiedFilePointer),
            SET_FILE_POINTER_MOVE_METHOD.FILE_CURRENT)!;

        result.Should().Be(1);
        modifiedFilePointer.Should().Be(filePointer + distanceToMove);

        distanceToMove = Random.Shared.NextInt64(int.MinValue, int.MaxValue);
        result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.SetFilePointerEx),
            receivedHandle,
            distanceToMove,
            (nint)(&modifiedFilePointer),
            SET_FILE_POINTER_MOVE_METHOD.FILE_END)!;

        result.Should().Be(1);
        modifiedFilePointer.Should().Be(fileLength + distanceToMove);

        _win32.DidNotReceiveWithAnyArgs().SetFilePointerEx(default, 0, default, default);
    }

    [Fact]
    public unsafe void ReadFileHook_CallsOriginal_WhenHandleIsNotBootConfig()
    {
        StartService();

        var expectedReturn = Random.Shared.Next() % 2 == 0;
        var receivedHandle = (HANDLE)Random.Shared.Next();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(_bootConfigFilePath);
        _win32.ReadFile(
                Arg.Any<HANDLE>(),
                Arg.Any<Pointer<byte>>(),
                Arg.Any<uint>(),
                Arg.Any<Pointer<uint>>(),
                Arg.Any<Pointer<NativeOverlapped>>())
            .ReturnsForAnyArgs((BOOL)expectedReturn);
        _pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "CreateFileW",
            fileNamePtr,
            0u,
            default(FILE_SHARE_MODE),
            null,
            default(FILE_CREATION_DISPOSITION),
            default(FILE_FLAGS_AND_ATTRIBUTES),
            default(HANDLE));

        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.ReadFile),
            receivedHandle,
            null,
            0u,
            null,
            null)!;

        result.Should().Be((BOOL)expectedReturn);
        _win32.Received(1).ReadFile(receivedHandle, default, 0, default, default);
    }

    [Fact]
    public unsafe void ReadFileHook_ReturnModifiedBootConfigl_WhenHandleIsBootConfig()
    {
        _bootConfigSettingsValue.WaitForNativeDebugger = GetRandomBool();
        _bootConfigSettingsValue.ScriptingRuntimeVersion = "latest";
        _bootConfigSettingsValue.VrEnabled = GetRandomBool();
        _bootConfigSettingsValue.HdrDisplayEnabled = GetRandomBool();
        _bootConfigSettingsValue.MaxNumLoopsNoJobBeforeGoingIdle = GetRandomInt();
        StartService();

        var expectedReturn = Random.Shared.Next() % 2 == 0;
        var receivedHandle = (HANDLE)Random.Shared.Next();
        var filePointer = (long)0;
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(_bootConfigFilePath);
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(receivedHandle);
        _win32.ReadFile(
                Arg.Any<HANDLE>(),
                Arg.Any<Pointer<byte>>(),
                Arg.Any<uint>(),
                Arg.Any<Pointer<uint>>(),
                Arg.Any<Pointer<NativeOverlapped>>())
            .ReturnsForAnyArgs((BOOL)expectedReturn);
        _pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "CreateFileW",
            fileNamePtr,
            0u,
            default(FILE_SHARE_MODE),
            null,
            default(FILE_CREATION_DISPOSITION),
            default(FILE_FLAGS_AND_ATTRIBUTES),
            default(HANDLE));
        _pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.SetFilePointerEx),
            receivedHandle,
            0,
            (nint)(&filePointer),
            SET_FILE_POINTER_MOVE_METHOD.FILE_END);
        var fileLength = filePointer;
        _pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.SetFilePointerEx),
            receivedHandle,
            0,
            (nint)(&filePointer),
            SET_FILE_POINTER_MOVE_METHOD.FILE_BEGIN);

        var bytes = Marshal.AllocHGlobal((int)fileLength);
        var bytesRead = (uint)0;
        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.ReadFile),
            receivedHandle,
            bytes,
            (uint)fileLength,
            (nint)(&bytesRead),
            null)!;

        result.Should().Be(1);
        var fileContent = Marshal.PtrToStringAnsi(bytes, (int)fileLength);
        Marshal.FreeHGlobal(bytes);

        fileContent.Should().Contain("gfx-enable-native-gfx-jobs=\n");
        fileContent.Should().Contain($"wait-for-native-debugger={(_bootConfigSettingsValue.WaitForNativeDebugger.Value ? 1 : 0)}\n");
        fileContent.Should().Contain($"scripting-runtime-version={_bootConfigSettingsValue.ScriptingRuntimeVersion}\n");
        fileContent.Should().Contain($"vr-enabled={(_bootConfigSettingsValue.VrEnabled.Value ? 1 : 0)}\n");
        fileContent.Should().Contain($"hdr-display-enabled={(_bootConfigSettingsValue.HdrDisplayEnabled.Value ? 1 : 0)}\n");
        fileContent.Should().Contain($"max-num-loops-no-job-before-going-idle={_bootConfigSettingsValue.MaxNumLoopsNoJobBeforeGoingIdle}\n");

        _win32.DidNotReceiveWithAnyArgs().ReadFile(default, default, 0, default, default);
    }

    [Fact]
    public unsafe void OnGameLifeCycle_UninstallPltHook_WhenMonoInitialisedEventReceived()
    {
        StartService();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(_bootConfigFilePath);
        _pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "CreateFileW",
            fileNamePtr,
            0u,
            default(FILE_SHARE_MODE),
            null,
            default(FILE_CREATION_DISPOSITION),
            default(FILE_FLAGS_AND_ATTRIBUTES),
            default(HANDLE));

        _gameLifecycleEvents.Publish(this, new()
        {
            LifeCycle = GameLifecycle.MonoInitialising
        });

        _pltHooksManager.Hooks.Should().BeEmpty();
    }
}