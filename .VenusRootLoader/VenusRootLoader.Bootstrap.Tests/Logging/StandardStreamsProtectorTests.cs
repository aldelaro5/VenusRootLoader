using AwesomeAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;
using Windows.Win32.Foundation;
using Windows.Win32.System.Console;

namespace VenusRootLoader.Bootstrap.Tests.Logging;

public sealed class StandardStreamsProtectorTests
{
    private readonly FakeLogger<StandardStreamsProtector> _logger = new();
    private readonly IWin32 _win32 = Substitute.For<IWin32>();
    private readonly TestPltHookManager _pltHookManager = new();
    private readonly IMonoInitLifeCycleEvents _monoInitLifeCycleEvents = new MonoInitLifeCycleEvents();

    private readonly GameExecutionContext _gameExecutionContext = new()
    {
        GameDir = "",
        DataDir = "",
        UnityPlayerDllFileName = "UnityPlayer.dll",
        IsWine = false
    };

    private readonly StandardStreamsProtector _sut;

    public StandardStreamsProtectorTests() => _sut = new(
        _logger,
        _pltHookManager,
        _gameExecutionContext,
        _monoInitLifeCycleEvents,
        _win32);

    [Fact]
    public void ProtectStreams_SetupHooks_WhenCalled()
    {
        _sut.ProtectStreams();
        _win32.Received(1).GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        _win32.Received(1).GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);
        _pltHookManager.Hooks.Should()
            .ContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(IWin32.CloseHandle)));
    }

    [Fact]
    public void CloseHandleHook_CallsOriginal_WhenHandleIsNotStdoutOrStderr()
    {
        HANDLE stdOutHandle = (HANDLE)Random.Shared.Next();
        HANDLE stdErrHandle = (HANDLE)Random.Shared.Next();
        HANDLE receivedHandle = (HANDLE)Random.Shared.Next();
        BOOL expectedResult = Random.Shared.Next() % 2 == 0;

        _win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE).Returns(stdOutHandle);
        _win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE).Returns(stdErrHandle);
        _win32.CloseHandle(Arg.Any<HANDLE>()).Returns(expectedResult);

        _sut.ProtectStreams();
        BOOL result = (BOOL)_pltHookManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(IWin32.CloseHandle),
            receivedHandle)!;

        result.Should().Be(expectedResult);
        _win32.Received(1).CloseHandle(receivedHandle);
    }

    [Theory]
    [InlineData(STD_HANDLE.STD_OUTPUT_HANDLE)]
    [InlineData(STD_HANDLE.STD_ERROR_HANDLE)]
    public void CloseHandleHook_ReturnsTrueWithoutCallingOriginal_WhenHandleIsStdoutOrStderr(STD_HANDLE stdHandle)
    {
        HANDLE stdOutHandle = (HANDLE)Random.Shared.Next();
        HANDLE stdErrHandle = (HANDLE)Random.Shared.Next();
        HANDLE receivedHandle = stdHandle == STD_HANDLE.STD_OUTPUT_HANDLE
            ? stdOutHandle
            : stdErrHandle;
        BOOL expectedResult = Random.Shared.Next() % 2 == 0;

        _win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE).Returns(stdOutHandle);
        _win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE).Returns(stdErrHandle);
        _win32.CloseHandle(Arg.Any<HANDLE>()).Returns(expectedResult);
        _win32.CompareObjectHandles(
                Arg.Any<HANDLE>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(c => (BOOL)(c.ArgAt<HANDLE>(0) == c.ArgAt<HANDLE>(1)));

        _sut.ProtectStreams();
        BOOL result = (BOOL)_pltHookManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(IWin32.CloseHandle),
            receivedHandle)!;

        result.Should().Be((BOOL)true);
        _win32.DidNotReceive().CloseHandle(receivedHandle);
    }

    [Fact]
    public void OnGameLifeCycle_UninstallPltHook_WhenMonoInitialisedEventReceived()
    {
        _sut.ProtectStreams();

        _monoInitLifeCycleEvents.Publish(this);

        _pltHookManager.Hooks
            .Should().NotContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(IWin32.CloseHandle)));
    }
}