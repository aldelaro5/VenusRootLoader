using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using AwesomeAssertions;
using NSubstitute;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;

namespace VenusRootLoader.Bootstrap.Tests.Shared;

public class CreateFileWSharedHookerTests
{
    private readonly TestPltHookManager _pltHooksManager = new();
    private readonly IWin32 _win32 = Substitute.For<IWin32>();
    private readonly GameExecutionContext _gameExecutionContext = new()
    {
        LibraryHandle = 0,
        GameDir = "",
        DataDir = "",
        UnityPlayerDllFileName = "UnityPlayer.dll",
        IsWine = false
    };

    private readonly CreateFileWSharedHooker _sut;

    public CreateFileWSharedHookerTests() => _sut = new(_pltHooksManager, _gameExecutionContext, _win32);

    [Fact]
    public unsafe void CreateFileWHook_CallsOriginal_WhenNoFileHooksAreRegistered()
    {
        var expectedReturn = (HANDLE)Random.Shared.Next();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalAnsi("someFile");
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

        _win32.Received(1).CreateFile(
            fileNamePtr,
            0,
            default,
            default,
            default,
            default,
            default);
        result.Should().Be(expectedReturn);

        Marshal.FreeHGlobal((nint)fileNamePtr.Value);
    }

    [Fact]
    public unsafe void CreateFileWHook_CallsOriginal_WhenAllFileHooksPredicatesReturnsFalse()
    {
        var expectedReturn = (HANDLE)Random.Shared.Next();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalAnsi("someFile");
        var hookedFileName1 = "file1";
        var hookedFileName2 = "file2";
        var hookedFileName3 = "file3";
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(expectedReturn);

        _sut.RegisterHook(hookedFileName1, _ => false, (out handle, _, _, _, _, _, _, _) => handle = HANDLE.Null);
        _sut.RegisterHook(hookedFileName2, _ => false, (out handle, _, _, _, _, _, _, _) => handle = HANDLE.Null);
        _sut.RegisterHook(hookedFileName3, _ => false, (out handle, _, _, _, _, _, _, _) => handle = HANDLE.Null);
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

        _win32.Received(1).CreateFile(
            fileNamePtr,
            0,
            default,
            default,
            default,
            default,
            default);
        result.Should().Be(expectedReturn);

        Marshal.FreeHGlobal((nint)fileNamePtr.Value);
    }

    [Fact]
    public unsafe void CreateFileWHook_CallsHooksWhosePredicateReturnsTrue_WhenSuchHooksExists()
    {
        var unexpectedReturn = (HANDLE)Random.Shared.Next();
        var expectedReturn = (HANDLE)Random.Shared.Next();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalAnsi("someFile");
        var hookedFileName1 = "file1";
        var hookedFileName2 = "file2";
        var hookedFileName3 = "file3";
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(unexpectedReturn);

        _sut.RegisterHook(hookedFileName1, _ => false, (out handle, _, _, _, _, _, _, _) => handle = unexpectedReturn);
        _sut.RegisterHook(hookedFileName2, _ => true, (out handle, _, _, _, _, _, _, _) => handle = expectedReturn);
        _sut.RegisterHook(hookedFileName3, _ => false, (out handle, _, _, _, _, _, _, _) => handle = unexpectedReturn);
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

        _win32.DidNotReceiveWithAnyArgs().CreateFile(
            default,
            0u,
            default,
            default,
            default,
            default,
            default);
        result.Should().Be(expectedReturn);

        Marshal.FreeHGlobal((nint)fileNamePtr.Value);
    }

    [Fact]
    public unsafe void CreateFileWHook_DoesNotCallHook_WhenItWasUnregistered()
    {
        var unexpectedReturn = (HANDLE)Random.Shared.Next();
        var expectedReturn = (HANDLE)Random.Shared.Next();
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalAnsi("someFile");
        var hookedFileName1 = "file1";
        var hookedFileName2 = "file2";
        var hookedFileName3 = "file3";
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(unexpectedReturn);

        _sut.RegisterHook(hookedFileName1, _ => true, (out handle, _, _, _, _, _, _, _) => handle = unexpectedReturn);
        _sut.RegisterHook(hookedFileName2, _ => true, (out handle, _, _, _, _, _, _, _) => handle = expectedReturn);
        _sut.RegisterHook(hookedFileName3, _ => false, (out handle, _, _, _, _, _, _, _) => handle = unexpectedReturn);

        _sut.UnregisterHook(hookedFileName1);
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
    
        _win32.DidNotReceiveWithAnyArgs().CreateFile(
            default,
            0u,
            default,
            default,
            default,
            default,
            default);
        result.Should().Be(expectedReturn);

        Marshal.FreeHGlobal((nint)fileNamePtr.Value);
    }

    [Fact]
    public unsafe void UnregisterHook_RemovesCreateFileWHook_WhenLastHookIsUnregistered()
    {
        var fileNamePtr = (PCWSTR)(char*)Marshal.StringToHGlobalAnsi("someFile");
        var hookedFileName1 = "file1";
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(HANDLE.Null);

        _sut.RegisterHook(hookedFileName1, _ => true, (out handle, _, _, _, _, _, _, _) => handle = HANDLE.Null);
        _sut.UnregisterHook(hookedFileName1);
        var result = _pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "CreateFileW",
            fileNamePtr,
            0u,
            default(FILE_SHARE_MODE),
            null,
            default(FILE_CREATION_DISPOSITION),
            default(FILE_FLAGS_AND_ATTRIBUTES),
            default(HANDLE))!;

        _pltHooksManager.Hooks.Should().BeEmpty();
        _win32.DidNotReceiveWithAnyArgs().CreateFile(
            default,
            0u,
            default,
            default,
            default,
            default,
            default);
        result.Should().BeNull();

        Marshal.FreeHGlobal((nint)fileNamePtr.Value);
    }
}