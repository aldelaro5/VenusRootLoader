using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.Extensions;

namespace VenusRootLoader.Bootstrap.Tests.Shared;

public class PltHooksManagerTests
{
    private readonly IPltHook _pltHookSub = Substitute.For<IPltHook>();
    private readonly ILogger<PltHooksManager> _loggerSub = Substitute.For<ILogger<PltHooksManager>>();
    private readonly IFileSystem  _fileSystem = Substitute.For<IFileSystem>();
    private readonly PltHooksManager _sut;

    public PltHooksManagerTests() => _sut = new(_loggerSub, _pltHookSub, _fileSystem);

    [Fact]
    public void InstallHook_OpensHookAndInstallHook_WhenCalledForTheFirstTimeForFilenameAndSucceeds()
    {
        string fileName = "filename";
        string functionName = "functionName";
        nint pointer = Random.Shared.Next();

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, functionName, pointer);

        _pltHookSub.Received(1).PlthookOpen(ref Arg.Any<nint>(), fileName);
        _pltHookSub.Received(1).PlthookReplace(Arg.Any<nint>(), functionName, pointer, ref Arg.Any<nint>());
    }

    [Fact]
    public void InstallHook_InstallHook_WhenCalledAfterFirstTimeForFilenameAndSucceeds()
    {
        string fileName = "filename";
        string functionName1 = "functionName1";
        string functionName2 = "functionName2";
        nint pointer = Random.Shared.Next();

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, functionName1, pointer);
        _sut.InstallHook(fileName, functionName2, pointer);

        _pltHookSub.Received(1).PlthookOpen(ref Arg.Any<nint>(), fileName);
        _pltHookSub.Received(1).PlthookReplace(Arg.Any<nint>(), functionName1, pointer, ref Arg.Any<nint>());
        _pltHookSub.Received(1).PlthookReplace(Arg.Any<nint>(), functionName2, pointer, ref Arg.Any<nint>());
    }

    [Fact]
    public void InstallHook_LogsTracesForEachHookPerFilename_WhenHookSucceedsAndTraceLogsAreEnabled()
    {
        string[] fileNames = ["filename1", "filename2"];
        string[] functionNames = ["functionName1", "functionName2"];
        nint pointer = Random.Shared.Next();

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);
        _loggerSub.IsEnabled(LogLevel.Trace).Returns(true);

        int fileNameAmount = 0;
        int functionNameAmount = 0;
        foreach (var fileName in fileNames)
        {
            fileNameAmount++;
            for (var j = 0; j < functionNames.Length; j++)
            {
                var functionName = functionNames[j];
                functionNameAmount++;
                _sut.InstallHook(fileName, functionName, pointer);
                if (j == 0)
                    _pltHookSub.Received(1).PlthookOpen(ref Arg.Any<nint>(), fileName);
                _pltHookSub.Received(1).PlthookReplace(Arg.Any<nint>(), functionName, pointer, ref Arg.Any<nint>());
                _loggerSub.ReceivedLog(1 + fileNameAmount + functionNameAmount, LogLevel.Trace);

                _pltHookSub.ClearReceivedCalls();
                _loggerSub.ClearReceivedCalls();
            }
        }
    }

    [Fact]
    public void InstallHook_LogsError_WhenPLtHookOpenFails()
    {
        string fileName = "filename";
        string functionName = "functionName";
        nint pointer = Random.Shared.Next();
        string errorString = "error";

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(false);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookError().ReturnsForAnyArgs(Marshal.StringToHGlobalUni(errorString));

        _sut.InstallHook(fileName, functionName, pointer);

        _pltHookSub.Received(1).PlthookOpen(ref Arg.Any<nint>(), fileName);
        _pltHookSub.DidNotReceiveWithAnyArgs().PlthookReplace(Arg.Any<nint>(), functionName, pointer, ref Arg.Any<nint>());
        _pltHookSub.Received(1).PlthookError();
        _loggerSub.ReceivedLog(1, LogLevel.Error, log => log.ToString()!.Contains(errorString));
    }

    [Fact]
    public void InstallHook_LogsError_WhenPLtHookReplaceFails()
    {
        string fileName = "filename";
        string functionName = "functionName";
        nint pointer = Random.Shared.Next();
        string errorString = "<some error>";

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(false);
        _pltHookSub.PlthookError().ReturnsForAnyArgs(Marshal.StringToHGlobalAuto(errorString));

        _sut.InstallHook(fileName, functionName, pointer);

        _pltHookSub.Received(1).PlthookOpen(ref Arg.Any<nint>(), fileName);
        _pltHookSub.Received(1).PlthookReplace(Arg.Any<nint>(), functionName, pointer, ref Arg.Any<nint>());
        _pltHookSub.Received(1).PlthookError();
        _loggerSub.ReceivedLog(1, LogLevel.Error, log => log.ToString()!.Contains(errorString));
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
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);

        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.DidNotReceive().PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            ref Arg.Is<nint>(x => x == nint.Zero));
    }

    [Fact]
    public void UninstallHook_DoesNothing_WhenFunctionWasNeverHookedForFilename()
    {
        string fileName = "filename";
        string functionName = "functionName";
        nint pointer = Random.Shared.Next();

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, "some other function", pointer);
        _pltHookSub.ClearReceivedCalls();
        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.DidNotReceive().PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            ref Arg.Is<nint>(x => x == nint.Zero));
    }

    [Fact]
    public void UninstallHook_UnhookAndClosePLtHook_WhenHookExistsAndIsLastHookForFilename()
    {
        string fileName = "filename";
        string functionName = "functionName";
        nint pointer = Random.Shared.Next();

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, functionName, pointer);
        _pltHookSub.ClearReceivedCalls();
        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            ref Arg.Is<nint>(x => x == nint.Zero));
        _pltHookSub.Received(1).PlthookClose(Arg.Any<nint>());
    }

    [Fact]
    public void UninstallHook_LogsActiveHooksAfterClose_WhenLastHookIsRemovedAndTraceLogsAreEnabled()
    {
        string fileName = "filename";
        string functionName = "functionName";
        nint pointer = Random.Shared.Next();

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);
        _loggerSub.IsEnabled(LogLevel.Trace).Returns(true);

        _sut.InstallHook(fileName, functionName, pointer);
        _pltHookSub.ClearReceivedCalls();
        _loggerSub.ClearReceivedCalls();
        _sut.UninstallHook(fileName, functionName);

        // Only header log since no more hooks exists
        _loggerSub.ReceivedLog(1, LogLevel.Trace);
    }

    [Fact]
    public void UninstallHook_UnhookWithoutClosingPltHook_WhenHookExistsAndIsNotLastHookForFilename()
    {
        string fileName = "filename";
        string functionName = "functionName";
        nint pointer = Random.Shared.Next();

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);

        _sut.InstallHook(fileName, functionName, pointer);
        _sut.InstallHook(fileName, "some other function", pointer);
        _pltHookSub.ClearReceivedCalls();
        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            ref Arg.Is<nint>(x => x == nint.Zero));
        _pltHookSub.DidNotReceiveWithAnyArgs().PlthookClose(Arg.Any<nint>());
    }

    [Fact]
    public void UninstallHook_LogsActiveHooksAfterUnhook_WhenOtherHooksExistAndTraceLogsAreEnabled()
    {
        string fileName = "filename";
        string functionName = "functionName";
        nint pointer = Random.Shared.Next();

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);
        _loggerSub.IsEnabled(LogLevel.Trace).Returns(true);

        _sut.InstallHook(fileName, functionName, pointer);
        _sut.InstallHook(fileName, "some other function", pointer);
        _pltHookSub.ClearReceivedCalls();
        _loggerSub.ClearReceivedCalls();
        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            ref Arg.Is<nint>(x => x == nint.Zero));
        _pltHookSub.DidNotReceiveWithAnyArgs().PlthookClose(Arg.Any<nint>());

        // The header, filename and function name
        _loggerSub.ReceivedLog(3, LogLevel.Trace);
    }

    [Fact]
    public void UninstallHook_LogsError_WhenUnhookingFails()
    {
        string fileName = "filename";
        string functionName = "functionName";
        nint pointer = Random.Shared.Next();
        string errorString = "<some error>";

        _pltHookSub.PlthookOpen(
                ref Arg.Any<nint>(),
                Arg.Any<string>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(true);
        _pltHookSub.PlthookError().ReturnsForAnyArgs(Marshal.StringToHGlobalAuto(errorString));

        _sut.InstallHook(fileName, functionName, pointer);
        _sut.InstallHook(fileName, "some other function", pointer);
        _pltHookSub.ClearReceivedCalls();
        _pltHookSub.Configure().PlthookReplace(
                Arg.Any<nint>(),
                Arg.Any<string>(),
                Arg.Any<nint>(),
                ref Arg.Any<nint>())
            .ReturnsForAnyArgs(false);

        _sut.UninstallHook(fileName, functionName);

        _pltHookSub.Received(1).PlthookReplace(
            Arg.Any<nint>(),
            functionName,
            Arg.Any<nint>(),
            ref Arg.Is<nint>(x => x == nint.Zero));
        _loggerSub.ReceivedLog(1, LogLevel.Error, log => log.ToString()!.Contains(errorString));
    }
}