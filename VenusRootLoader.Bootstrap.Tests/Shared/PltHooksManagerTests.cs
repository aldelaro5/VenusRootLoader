using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.Extensions;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Tests.Shared;

public class PltHooksManagerTests
{
    private readonly IPltHook _pltHookSub = Substitute.For<IPltHook>();
    private readonly FakeLogger<PltHooksManager> _loggerSub = new();
    private readonly IFileSystem  _fileSystem = Substitute.For<IFileSystem>();
    private readonly PltHooksManager _sut;

    public PltHooksManagerTests() => _sut = new(_loggerSub, _pltHookSub, _fileSystem);

    [Fact]
    public void InstallHook_OpensHookAndInstallHook_WhenCalledForTheFirstTimeForFilenameAndSucceeds()
    {
        string fileName = "filename";
        string functionName = "functionName";
        Action hook = () => {};

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, functionName, hook);

        _pltHookSub.Received(1).PlthookOpen(Arg.Any<Pointer<nint>>(), fileName);
        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Is<nint>(x => Marshal.GetDelegateForFunctionPointer<Action>(x) == hook),
            Arg.Any<Pointer<nint>>());
    }

    [Fact]
    public void InstallHook_InstallHook_WhenCalledAfterFirstTimeForFilenameAndSucceeds()
    {
        string fileName = "filename";
        string functionName1 = "functionName1";
        string functionName2 = "functionName2";
        Action hook = () => {};

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, functionName1, hook);
        _sut.InstallHook(fileName, functionName2, hook);

        _pltHookSub.Received(1).PlthookOpen(Arg.Any<Pointer<nint>>(), fileName);
        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName1,
            Arg.Is<nint>(x => Marshal.GetDelegateForFunctionPointer<Action>(x) == hook),
            Arg.Any<Pointer<nint>>());
        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName2,
            Arg.Is<nint>(x => Marshal.GetDelegateForFunctionPointer<Action>(x) == hook),
            Arg.Any<Pointer<nint>>());
    }

    [Fact]
    public void InstallHook_LogsTracesForEachHookPerFilename_WhenHookSucceedsAndTraceLogsAreEnabled()
    {
        string[] fileNames = ["filename1", "filename2"];
        string[] functionNames = ["functionName1", "functionName2"];
        Action hook = () => {};

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);
        _loggerSub.ControlLevel(LogLevel.Trace, true);

        int fileNameAmount = 0;
        int functionNameAmount = 0;
        foreach (var fileName in fileNames)
        {
            fileNameAmount++;
            for (var j = 0; j < functionNames.Length; j++)
            {
                var functionName = functionNames[j];
                functionNameAmount++;
                _sut.InstallHook(fileName, functionName, hook);
                if (j == 0)
                    _pltHookSub.Received(1).PlthookOpen(Arg.Any<Pointer<nint>>(), fileName);
                _pltHookSub.Received(1).PlthookReplace(
                    Arg.Any<nint>(),
                    functionName,
                    Arg.Is<nint>(x => Marshal.GetDelegateForFunctionPointer<Action>(x) == hook),
                    Arg.Any<Pointer<nint>>());
                _loggerSub.Collector.GetSnapshot().Where(r => r.Level == LogLevel.Trace)
                    .Should().HaveCount(1 + fileNameAmount + functionNameAmount);

                _pltHookSub.ClearReceivedCalls();
                _loggerSub.Collector.Clear();
            }
        }
    }

    [Fact]
    public void InstallHook_LogsError_WhenPLtHookOpenFails()
    {
        string fileName = "filename";
        string functionName = "functionName";
        Action hook = () => {};
        string errorString = "error";
        var errorStringPtr = Marshal.StringToHGlobalUni(errorString);

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(false);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookError().ReturnsForAnyArgs(errorStringPtr);

        _sut.InstallHook(fileName, functionName, hook);

        _pltHookSub.Received(1).PlthookOpen(Arg.Any<Pointer<nint>>(), fileName);
        _pltHookSub.DidNotReceiveWithAnyArgs().PlthookReplace(Arg.Any<nint>(), Arg.Any<string>(), Arg.Any<nint>(), Arg.Any<Pointer<nint>>());
        _pltHookSub.Received(1).PlthookError();
        _loggerSub.LatestRecord.Should().Match<FakeLogRecord>(r => r.Level == LogLevel.Error && r.Message.Contains(errorString));

        Marshal.FreeHGlobal(errorStringPtr);
    }

    [Fact]
    public void InstallHook_LogsError_WhenPLtHookReplaceFails()
    {
        string fileName = "filename";
        string functionName = "functionName";
        Action hook = () => {};
        string errorString = "<some error>";

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(false);
        _pltHookSub.PlthookError().ReturnsForAnyArgs(Marshal.StringToHGlobalAuto(errorString));

        _sut.InstallHook(fileName, functionName, hook);

        _pltHookSub.Received(1).PlthookOpen(Arg.Any<Pointer<nint>>(), fileName);
        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Is<nint>(x => Marshal.GetDelegateForFunctionPointer<Action>(x) == hook),
            Arg.Any<Pointer<nint>>());
        _pltHookSub.Received(1).PlthookError();
        _loggerSub.LatestRecord.Should().Match<FakeLogRecord>(r => r.Level == LogLevel.Error && r.Message.Contains(errorString));
    }

    [Fact]
    public void UninstallHook_DoesNothing_WhenHookWasNeverOpenedForFilename()
    {
        string fileName = "filename";
        string functionName = "functionName";

        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);

        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.DidNotReceive().PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            Arg.Any<Pointer<nint>>());
    }

    [Fact]
    public void UninstallHook_DoesNothing_WhenFunctionWasNeverHookedForFilename()
    {
        string fileName = "filename";
        string functionName = "functionName";
        Action hook = () => {};

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, "some other function", hook);
        _pltHookSub.ClearReceivedCalls();
        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.DidNotReceive().PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            Arg.Any<Pointer<nint>>());
    }

    [Fact]
    public void UninstallHook_UnhookAndClosePLtHook_WhenHookExistsAndIsLastHookForFilename()
    {
        string fileName = "filename";
        string functionName = "functionName";
        Action hook = () => {};

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, functionName, hook);
        _pltHookSub.ClearReceivedCalls();
        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            Arg.Any<Pointer<nint>>());
        _pltHookSub.Received(1).PlthookClose(Arg.Any<nint>());
    }

    [Fact]
    public void UninstallHook_LogsActiveHooksAfterClose_WhenLastHookIsRemovedAndTraceLogsAreEnabled()
    {
        string fileName = "filename";
        string functionName = "functionName";
        Action hook = () => {};

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);
        _loggerSub.ControlLevel(LogLevel.Trace, true);

        _sut.InstallHook(fileName, functionName, hook);
        _pltHookSub.ClearReceivedCalls();
        _loggerSub.Collector.Clear();
        _sut.UninstallHook(fileName, functionName);

        // Only header log since no more hooks exists
        _loggerSub.Collector.GetSnapshot().Should().ContainSingle(r => r.Level == LogLevel.Trace);
    }

    [Fact]
    public void UninstallHook_UnhookWithoutClosingPltHook_WhenHookExistsAndIsNotLastHookForFilename()
    {
        string fileName = "filename";
        string functionName = "functionName";
        Action hook = () => {};

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, functionName, hook);
        _sut.InstallHook(fileName, "some other function", hook);
        _pltHookSub.ClearReceivedCalls();
        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            Arg.Any<Pointer<nint>>());
        _pltHookSub.DidNotReceiveWithAnyArgs().PlthookClose(Arg.Any<nint>());
    }

    [Fact]
    public void UninstallHook_LogsActiveHooksAfterUnhook_WhenOtherHooksExistAndTraceLogsAreEnabled()
    {
        string fileName = "filename";
        string functionName = "functionName";
        Action hook = () => {};

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);
        _loggerSub.ControlLevel(LogLevel.Trace, true);

        _sut.InstallHook(fileName, functionName, hook);
        _sut.InstallHook(fileName, "some other function", hook);
        _pltHookSub.ClearReceivedCalls();
        _loggerSub.Collector.Clear();
        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            Arg.Any<Pointer<nint>>());
        _pltHookSub.DidNotReceiveWithAnyArgs().PlthookClose(Arg.Any<nint>());

        // The header, filename and function name
        _loggerSub.Collector.GetSnapshot().Where(r => r.Level == LogLevel.Trace)
            .Should().HaveCount(3);
    }

    [Fact]
    public void UninstallHook_LogsError_WhenUnhookingFails()
    {
        string fileName = "filename";
        string functionName = "functionName";
        Action hook = () => {};
        string errorString = "<some error>";

        _pltHookSub.PlthookOpen(
                Arg.Any<Pointer<nint>>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookError().ReturnsForAnyArgs(Marshal.StringToHGlobalAuto(errorString));

        _sut.InstallHook(fileName, functionName, hook);
        _sut.InstallHook(fileName, "some other function", hook);
        _pltHookSub.ClearReceivedCalls();
        _pltHookSub.Configure().PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                Arg.Any<Pointer<nint>>())
            .ReturnsForAnyArgs(false);

        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            Arg.Any<Pointer<nint>>());
        _loggerSub.LatestRecord.Should().Match<FakeLogRecord>(r => r.Level == LogLevel.Error && r.Message.Contains(errorString));
    }
}