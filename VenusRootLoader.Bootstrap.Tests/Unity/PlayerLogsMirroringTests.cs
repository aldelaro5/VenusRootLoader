using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;
using VenusRootLoader.Bootstrap.Unity;

namespace VenusRootLoader.Bootstrap.Tests.Unity;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class PlayerLogsMirroringTests
{
    private readonly FakeLogger _logger = new();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly TestPltHookManager _pltHooksManager = new();
    private readonly IOptions<LoggingSettings> _loggingSettings = Substitute.For<IOptions<LoggingSettings>>();
    private readonly LoggingSettings _loggingSettingsValue = new()
    {
        ConsoleLoggerSettings = new(),
        DiskFileLoggerSettings = new(),
        IncludeUnityLogs = true
    };
    private readonly IWin32 _win32 = Substitute.For<IWin32>();
    private readonly TestGameLifecycleEvents _gameLifecycleEvents = new();
    private readonly CreateFileWSharedHooker _createFileWSharedHooker;
    private readonly GameExecutionContext _gameExecutionContext = new()
    {
        LibraryHandle = 0,
        GameDir = "",
        DataDir = "",
        UnityPlayerDllFileName = "UnityPlayer.dll",
        IsWine = false
    };

    public PlayerLogsMirroringTests()
    {
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_logger);
        _createFileWSharedHooker = new(_pltHooksManager, _gameExecutionContext, _win32);
        _loggingSettings.Value.Returns(_loggingSettingsValue);
    }

    private void StartService()
    {
        var sut = new PlayerLogsMirroring(
            _loggerFactory,
            _pltHooksManager,
            _createFileWSharedHooker,
            _gameExecutionContext,
            _loggingSettings,
            _gameLifecycleEvents,
            _win32);
        sut.StartAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void StartAsync_SetupHooks_WhenCalled()
    {
        StartService();

        _win32.Received(1).GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        _win32.Received(1).GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);
        _pltHooksManager.Hooks.Should().ContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.WriteFile)));
        _pltHooksManager.Hooks.Should().ContainKey((_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW"));
    }

    [Theory]
    [InlineData("stuff/output_log.txt")]
    [InlineData("things/Player.log")]
    public unsafe void CreateFileHook_UnregistersHook_WhenCalledWithPlayerLogsFilename(string filename)
    {
        StartService();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(filename);
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

        _pltHooksManager.Hooks.Should().NotContainKey((_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW"));
        _win32.Received(1).CreateFile(
            Arg.Is<PCWSTR>(s => string.Equals(s.ToString(), filename, StringComparison.Ordinal)),
            0, default, default, default, default, default);
    }

    [Fact]
    public unsafe void CreateFileHook_DoesNotUnregistersHook_WhenCalledWithoutPlayerLogsFilename()
    {
        StartService();
        var filename = "SomeOtherFiles.txt";
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(filename);
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

        _pltHooksManager.Hooks.Should().ContainKey((_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW"));
        _win32.Received(1).CreateFile(
            Arg.Is<PCWSTR>(s => string.Equals(s.ToString(), filename, StringComparison.Ordinal)),
            0, default, default, default, default, default);
    }

    [Fact]
    public unsafe void WriteFileHook_CallsOriginalWithoutLogging_WhenHandleIsNotPlayerLogFileOrStdHandle()
    {
        StartService();
        var filename = "output_log.txt";
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(filename);
        var handleReceived = (HANDLE)Random.Shared.Next();
        var expectedReturn = (BOOL)(Random.Shared.Next() == 0);
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
        _win32.WriteFile(
                Arg.Any<HANDLE>(),
                Arg.Any<Pointer<byte>>(),
                Arg.Any<uint>(),
                Arg.Any<Pointer<uint>>(),
                Arg.Any<Pointer<NativeOverlapped>>())
            .ReturnsForAnyArgs(expectedReturn);

        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.WriteFile),
            handleReceived,
            null,
            0u,
            null,
            null)!;

        result.Should().Be(expectedReturn);
        _win32.Received(1).CreateFile(
            Arg.Is<PCWSTR>(s => string.Equals(s.ToString(), filename, StringComparison.Ordinal)),
            0, default, default, default, default, default);
        _win32.Received(1).WriteFile(handleReceived, default, 0u, default, default);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public unsafe void WriteFileHook_CallsOriginalWithoutLogging_WhenLogMirroringIsDisabledAndHandleIsPlayerLogFile()
    {
        _loggingSettingsValue.IncludeUnityLogs = false;
        StartService();
        var filename = "output_log.txt";
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(filename);
        var handlePlayerLogFile = (HANDLE)Random.Shared.Next();
        var expectedReturn = (BOOL)(Random.Shared.Next() == 0);
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(handlePlayerLogFile);
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
        _win32.WriteFile(
                Arg.Any<HANDLE>(),
                Arg.Any<Pointer<byte>>(),
                Arg.Any<uint>(),
                Arg.Any<Pointer<uint>>(),
                Arg.Any<Pointer<NativeOverlapped>>())
            .ReturnsForAnyArgs(expectedReturn);

        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.WriteFile),
            handlePlayerLogFile,
            null,
            0u,
            null,
            null)!;

        result.Should().Be(expectedReturn);
        _win32.Received(1).CreateFile(
            Arg.Is<PCWSTR>(s => string.Equals(s.ToString(), filename, StringComparison.Ordinal)),
            0, default, default, default, default, default);
        _win32.Received(1).WriteFile(handlePlayerLogFile, default, 0u, default, default);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public unsafe void WriteFileHook_CallsOriginalWithLogging_WhenLogMirroringIsEnabledAndHandleIsPlayerLogFile()
    {
        StartService();
        var filename = "output_log.txt";
        var message = "Some logging message\r\n";
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni(filename);
        var messagePtr = (byte*)Marshal.StringToHGlobalAnsi(message);
        var handlePlayerLogFile = (HANDLE)Random.Shared.Next();
        var expectedReturn = (BOOL)(Random.Shared.Next() == 0);
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(handlePlayerLogFile);
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
        _win32.WriteFile(
                Arg.Any<HANDLE>(),
                Arg.Any<Pointer<byte>>(),
                Arg.Any<uint>(),
                Arg.Any<Pointer<uint>>(),
                Arg.Any<Pointer<NativeOverlapped>>())
            .ReturnsForAnyArgs(expectedReturn);

        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.WriteFile),
            handlePlayerLogFile,
            (nint)messagePtr,
            (uint)message.Length,
            null,
            null)!;

        result.Should().Be(expectedReturn);
        _win32.Received(1).CreateFile(
            Arg.Is<PCWSTR>(s => string.Equals(s.ToString(), filename, StringComparison.Ordinal)),
            0, default, default, default, default, default);
        _win32.Received(1).WriteFile(handlePlayerLogFile, Arg.Any<Pointer<byte>>(), (uint)message.Length, default, default);
        _logger.Collector.Count.Should().Be(1);
        _logger.LatestRecord.Level.Should().Be(LogLevel.Trace);
        _logger.LatestRecord.Message.Should().Be(message.TrimEnd("\r\n").ToString());
    }

    [Theory]
    [InlineData(STD_HANDLE.STD_OUTPUT_HANDLE)]
    [InlineData(STD_HANDLE.STD_ERROR_HANDLE)]
    public void WriteFileHook_DoesNotCallsOriginalWithoutLogging_WhenLogMirroringIsDisabledAndHandleIsStdHandle(STD_HANDLE stdHandle)
    {
        _loggingSettingsValue.IncludeUnityLogs = false;
        var handle = (HANDLE)Random.Shared.Next();
        _win32.GetStdHandle(stdHandle).Returns(handle);
        StartService();

        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.WriteFile),
            handle,
            null,
            0u,
            null,
            null)!;

        result.Should().Be(1);
        _win32.DidNotReceiveWithAnyArgs().WriteFile(default, default, 0u, default, default);
        _logger.Collector.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(STD_HANDLE.STD_OUTPUT_HANDLE)]
    [InlineData(STD_HANDLE.STD_ERROR_HANDLE)]
    public unsafe void WriteFileHook_DoesNotCallsOriginalWithLogging_WhenLogMirroringIsEnabledAndHandleIsStdHandle(STD_HANDLE stdHandle)
    {
        var handle = (HANDLE)Random.Shared.Next();
        _win32.GetStdHandle(stdHandle).Returns(handle);
        StartService();

        var message = "Some logging message\r\n";
        var messagePtr = (byte*)Marshal.StringToHGlobalAnsi(message);
        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.WriteFile),
            handle,
            (nint)messagePtr,
            (uint)message.Length,
            null,
            null)!;

        result.Should().Be(1);
        _win32.DidNotReceiveWithAnyArgs().WriteFile(default, default, 0u, default, default);
        _logger.Collector.Count.Should().Be(1);
        _logger.LatestRecord.Level.Should().Be(LogLevel.Trace);
        _logger.LatestRecord.Message.Should().Be(message.TrimEnd("\r\n").ToString());
    }

    [Fact]
    public unsafe void OnGameLifeCycle_UninstallPltHook_WhenMonoInitialisedEventReceived()
    {
        StartService();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalUni("output_log.txt");
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

        _pltHooksManager.Hooks.Should().NotContainKey((_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW"));
    }
}