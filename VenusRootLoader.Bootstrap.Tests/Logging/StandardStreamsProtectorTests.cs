using AwesomeAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;
using Windows.Win32.Foundation;
using Windows.Win32.System.Console;

namespace VenusRootLoader.Bootstrap.Tests.Logging;

public class StandardStreamsProtectorTests
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
    public async Task StartAsync_SetupHooks_WhenCalled()
    {
        await _sut.StartAsync(CancellationToken.None);

        _win32.Received(1).GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        _win32.Received(1).GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);
        _pltHookManager.Hooks.Should()
            .ContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(IWin32.CloseHandle)));
    }

    [Fact]
    public async Task CloseHandleHook_CallsOriginal_WhenHandleIsNotStdoutOrStderr()
    {
        var stdOutHandle = (HANDLE)Random.Shared.Next();
        var stdErrHandle = (HANDLE)Random.Shared.Next();
        var receivedHandle = (HANDLE)Random.Shared.Next();
        var expectedResult = (BOOL)(Random.Shared.Next() % 2 == 0);

        _win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE).Returns(stdOutHandle);
        _win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE).Returns(stdErrHandle);
        _win32.CloseHandle(Arg.Any<HANDLE>()).Returns(expectedResult);

        await _sut.StartAsync(CancellationToken.None);
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
    public async Task CloseHandleHook_ReturnsTrueWithoutCallingOriginal_WhenHandleIsStdoutOrStderr(STD_HANDLE stdHandle)
    {
        var stdOutHandle = (HANDLE)Random.Shared.Next();
        var stdErrHandle = (HANDLE)Random.Shared.Next();
        var receivedHandle = stdHandle == STD_HANDLE.STD_OUTPUT_HANDLE
            ? stdOutHandle
            : stdErrHandle;
        var expectedResult = (BOOL)(Random.Shared.Next() % 2 == 0);

        _win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE).Returns(stdOutHandle);
        _win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE).Returns(stdErrHandle);
        _win32.CloseHandle(Arg.Any<HANDLE>()).Returns(expectedResult);
        _win32.CompareObjectHandles(
                Arg.Any<HANDLE>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(c => (BOOL)(c.ArgAt<HANDLE>(0) == c.ArgAt<HANDLE>(1)));

        await _sut.StartAsync(CancellationToken.None);
        BOOL result = (BOOL)_pltHookManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(IWin32.CloseHandle),
            receivedHandle)!;

        result.Should().Be((BOOL)true);
        _win32.DidNotReceive().CloseHandle(receivedHandle);
    }

    [Fact]
    public async Task OnGameLifeCycle_UninstallPltHook_WhenMonoInitialisedEventReceived()
    {
        await _sut.StartAsync(CancellationToken.None);

        _monoInitLifeCycleEvents.Publish(this);

        _pltHookManager.Hooks
            .Should().NotContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(IWin32.CloseHandle)));
    }
}