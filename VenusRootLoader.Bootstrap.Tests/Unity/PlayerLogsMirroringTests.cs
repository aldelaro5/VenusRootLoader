using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;
using VenusRootLoader.Bootstrap.Unity;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;

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
    private readonly IGameLifecycleEvents _gameLifecycleEvents = new GameLifecycleEvents();
    private readonly TestCreateFileWSharedHooker _createFileWSharedHooker = new();

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
        _pltHooksManager.Hooks.Should()
            .ContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.WriteFile)));
        _createFileWSharedHooker.Hooks.Should().ContainKey(nameof(PlayerLogsMirroring));
    }

    [Theory]
    [InlineData("stuff/output_log.txt")]
    [InlineData("things/Player.log")]
    public unsafe void CreateFileHook_UnregistersHook_WhenCalledWithPlayerLogsFilename(string filename)
    {
        StartService();
        var fileNamePtr = (char*)Marshal.StringToHGlobalUni(filename);
        _createFileWSharedHooker.SimulateHook(fileNamePtr);

        _pltHooksManager.Hooks.Should().NotContainKey((_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW"));
        _win32.Received(1).CreateFile(
            Arg.Is<PCWSTR>(s => string.Equals(s.ToString(), filename, StringComparison.Ordinal)),
            0,
            default,
            default,
            default,
            default,
            default);

        Marshal.FreeHGlobal((nint)fileNamePtr);
    }

    [Fact]
    public unsafe void CreateFileHook_DoesNotUnregistersHook_WhenCalledWithoutPlayerLogsFilename()
    {
        StartService();
        var filename = "SomeOtherFiles.txt";
        var fileNamePtr = (char*)Marshal.StringToHGlobalUni(filename);

        _createFileWSharedHooker.SimulateHook(fileNamePtr);

        _createFileWSharedHooker.Hooks.Should().ContainKey(nameof(PlayerLogsMirroring));

        Marshal.FreeHGlobal((nint)fileNamePtr);
    }

    [Fact]
    public unsafe void WriteFileHook_CallsOriginalWithoutLogging_WhenHandleIsNotPlayerLogFileOrStdHandle()
    {
        StartService();
        var filename = "output_log.txt";
        var fileNamePtr = (char*)Marshal.StringToHGlobalUni(filename);
        var handleReceived = (HANDLE)Random.Shared.Next();
        var expectedReturn = (BOOL)(Random.Shared.Next() == 0);
        _createFileWSharedHooker.SimulateHook(fileNamePtr);

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
            0,
            default,
            default,
            default,
            default,
            default);
        _win32.Received(1).WriteFile(handleReceived, default, 0u, default, default);
        _logger.Collector.Count.Should().Be(0);

        Marshal.FreeHGlobal((nint)fileNamePtr);
    }

    [Fact]
    public unsafe void WriteFileHook_CallsOriginalWithoutLogging_WhenLogMirroringIsDisabledAndHandleIsPlayerLogFile()
    {
        _loggingSettingsValue.IncludeUnityLogs = false;
        StartService();
        var filename = "output_log.txt";
        var fileNamePtr = (char*)Marshal.StringToHGlobalUni(filename);
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
        _createFileWSharedHooker.SimulateHook(fileNamePtr);
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
            0,
            default,
            default,
            default,
            default,
            default);
        _win32.Received(1).WriteFile(handlePlayerLogFile, default, 0u, default, default);
        _logger.Collector.Count.Should().Be(0);

        Marshal.FreeHGlobal((nint)fileNamePtr);
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
        _createFileWSharedHooker.SimulateHook(fileNamePtr);
        _win32.WriteFile(
                Arg.Any<HANDLE>(),
                Arg.Any<Pointer<byte>>(),
                Arg.Any<uint>(),
                Arg.Any<Pointer<uint>>(),
                Arg.Any<Pointer<NativeOverlapped>>())
            .ReturnsForAnyArgs(expectedReturn);
        _win32.CompareObjectHandles(
                Arg.Any<HANDLE>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(c => (BOOL)(c.ArgAt<HANDLE>(0) == c.ArgAt<HANDLE>(1)));

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
            0,
            default,
            default,
            default,
            default,
            default);
        _win32.Received(1).WriteFile(
            handlePlayerLogFile,
            Arg.Any<Pointer<byte>>(),
            (uint)message.Length,
            default,
            default);
        _logger.Collector.Count.Should().Be(1);
        _logger.LatestRecord.Level.Should().Be(LogLevel.Trace);
        _logger.LatestRecord.Message.Should().Be(message.TrimEnd("\r\n").ToString());

        Marshal.FreeHGlobal((nint)messagePtr);
    }

    [Theory]
    [InlineData(STD_HANDLE.STD_OUTPUT_HANDLE)]
    [InlineData(STD_HANDLE.STD_ERROR_HANDLE)]
    public void WriteFileHook_DoesNotCallsOriginalWithoutLogging_WhenLogMirroringIsDisabledAndHandleIsStdHandle(
        STD_HANDLE stdHandle)
    {
        _loggingSettingsValue.IncludeUnityLogs = false;
        var handle = (HANDLE)Random.Shared.Next();
        _win32.GetStdHandle(stdHandle).Returns(handle);
        _win32.CompareObjectHandles(
                Arg.Any<HANDLE>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(c => (BOOL)(c.ArgAt<HANDLE>(0) == c.ArgAt<HANDLE>(1)));
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
    public unsafe void WriteFileHook_DoesNotCallsOriginalWithLogging_WhenLogMirroringIsEnabledAndHandleIsStdHandle(
        STD_HANDLE stdHandle)
    {
        var handle = (HANDLE)Random.Shared.Next();
        _win32.GetStdHandle(stdHandle).Returns(handle);
        StartService();

        var message = "Some logging message\r\n";
        var messagePtr = (byte*)Marshal.StringToHGlobalAnsi(message);
        _win32.CompareObjectHandles(
                Arg.Any<HANDLE>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(c => (BOOL)(c.ArgAt<HANDLE>(0) == c.ArgAt<HANDLE>(1)));
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

        Marshal.FreeHGlobal((nint)messagePtr);
    }

    [Fact]
    public unsafe void OnGameLifeCycle_UninstallPltHook_WhenMonoInitialisedEventReceived()
    {
        StartService();
        var fileNamePtr = (char*)Marshal.StringToHGlobalUni("output_log.txt");
        _createFileWSharedHooker.SimulateHook(fileNamePtr);

        _gameLifecycleEvents.Publish(this);

        _pltHooksManager.Hooks.Should().NotContainKey((_gameExecutionContext.UnityPlayerDllFileName, "CreateFileW"));

        Marshal.FreeHGlobal((nint)fileNamePtr);
    }
}