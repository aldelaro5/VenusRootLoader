using AssetsTools.NET.Extra;
using AwesomeAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.InteropServices;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;
using VenusRootLoader.Bootstrap.Unity;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;

namespace VenusRootLoader.Bootstrap.Tests.Unity;

public class SplashScreenSkipperTests : IDisposable
{
    private readonly ILogger<SplashScreenSkipper> _logger = Substitute.For<ILogger<SplashScreenSkipper>>();
    private readonly IOptions<GlobalSettings> _globalSettings = Substitute.For<IOptions<GlobalSettings>>();
    private readonly IHostEnvironment _hostEnvironment = Substitute.For<IHostEnvironment>();
    private readonly IWin32 _win32 = Substitute.For<IWin32>();
    private readonly IFileSystem _fileSystem = new FileSystem();
    private readonly TestPltHookManager _pltHookManager = new();
    private readonly TestCreateFileWSharedHooker _createFileWSharedHooker = new();
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly GlobalSettings _globalSettingsValue = new()
    {
        DisableVrl = false,
        SkipUnitySplashScreen = true
    };
    private readonly string _dataUnity3dPath;
    private readonly string _pathModifiedBundle;

    public SplashScreenSkipperTests()
    {
        _hostEnvironment.ContentRootPath.Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        _globalSettings.Value.Returns(_globalSettingsValue);
        _gameExecutionContext = new()
        {
            LibraryHandle = 0,
            GameDir = "",
            DataDir = Path.Combine(_hostEnvironment.ContentRootPath, "UnityCompiledProject/VenusRootLoaderTestProject_Data"),
            UnityPlayerDllFileName = "UnityPlayer.dll",
            IsWine = false
        };
        _dataUnity3dPath = Path.Combine(_gameExecutionContext.DataDir, "data.unity3d");
        _pathModifiedBundle = Path.Combine(_hostEnvironment.ContentRootPath, "VenusRootLoader", "data.unity3d.modified");
        DeleteRealTestFiles();
    }

    private void DeleteRealTestFiles()
    {
        if (_fileSystem.File.Exists($"{_pathModifiedBundle}.uncompressed"))
            File.Delete($"{_pathModifiedBundle}.uncompressed");
        if (_fileSystem.File.Exists($"{_pathModifiedBundle}"))
            File.Delete($"{_pathModifiedBundle}");
    }

    private void StartService()
    {
        var sut = new SplashScreenSkipper(_logger, _createFileWSharedHooker, _gameExecutionContext, _globalSettings, _hostEnvironment, _win32, _fileSystem);
        sut.StartAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public unsafe void CreateFileHook_DoesNothing_WhenSkipperIsDisabled()
    {
        _globalSettingsValue.SkipUnitySplashScreen = false;
        StartService();

        var fileNamePtr = (char*)Marshal.StringToHGlobalUni(_dataUnity3dPath);

        _createFileWSharedHooker.SimulateHook(fileNamePtr);

        File.Exists(_pathModifiedBundle).Should().BeFalse();

        Marshal.FreeHGlobal((nint)fileNamePtr);
    }

    [Fact]
    public unsafe void CreateFileHook_CreatesModifiedBundle_WhenSkipperIsEnabled()
    {
        StartService();

        var expectedReturn = (HANDLE)Random.Shared.Next();
        var fileNamePtr = (char*)Marshal.StringToHGlobalUni(_dataUnity3dPath);
        _win32.CreateFile(
                Arg.Any<PCWSTR>(),
                Arg.Any<uint>(),
                Arg.Any<FILE_SHARE_MODE>(),
                Arg.Any<Pointer<SECURITY_ATTRIBUTES>>(),
                Arg.Any<FILE_CREATION_DISPOSITION>(),
                Arg.Any<FILE_FLAGS_AND_ATTRIBUTES>(),
                Arg.Any<HANDLE>())
            .ReturnsForAnyArgs(expectedReturn);

        var result = _createFileWSharedHooker.SimulateHook(fileNamePtr);

        result.Should().Be(expectedReturn);
        File.Exists(_pathModifiedBundle).Should().BeTrue();
        AssertModifiedBundleIsCorrect();
        _win32.Received(1).CreateFile(
            Arg.Is<PCWSTR>(s => string.Equals(s.ToString(), _pathModifiedBundle, StringComparison.Ordinal)),
            0, default, default, default, default, default);

        Marshal.FreeHGlobal((nint)fileNamePtr);
    }

    [Fact]
    public unsafe void CreateFileHook_UnregisterHook_WhenCalledTwice()
    {
        StartService();

        var fileNamePtr = (char*)Marshal.StringToHGlobalUni(_dataUnity3dPath);

        _createFileWSharedHooker.SimulateHook(fileNamePtr);
        _createFileWSharedHooker.SimulateHook(fileNamePtr);

        File.Exists(_pathModifiedBundle).Should().BeTrue();
        _win32.Received(2).CreateFile(
            Arg.Is<PCWSTR>(s => string.Equals(s.ToString(), _pathModifiedBundle, StringComparison.Ordinal)),
            0, default, default, default, default, default);
        _pltHookManager.Hooks.Should().BeEmpty();

        Marshal.FreeHGlobal((nint)fileNamePtr);
    }

    private void AssertModifiedBundleIsCorrect()
    {
        var manager = new AssetsManager();
        manager.LoadClassPackage(Path.Combine(_hostEnvironment.ContentRootPath, "VenusRootLoader", "classdata.tpk"));
        var bundleInstance = manager.LoadBundleFile(_pathModifiedBundle);
        var assetsFileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, 0);
        var assetFile = assetsFileInstance.file;
        manager.LoadClassDatabaseFromPackage(assetFile.Metadata.UnityVersion);

        var playerSettingAsset = assetFile.GetAssetInfo(1);
        var playerSettingsTypeValueField = manager.GetBaseField(assetsFileInstance, playerSettingAsset);
        playerSettingsTypeValueField["m_ShowUnitySplashScreen"].AsBool.Should().BeFalse();

        var buildSettingAsset = assetFile.GetAssetInfo(11);
        var buildSettingsTypeValueField = manager.GetBaseField(assetsFileInstance, buildSettingAsset);
        buildSettingsTypeValueField["hasPROVersion"].AsBool.Should().BeTrue();
    }

    public void Dispose() => DeleteRealTestFiles();
}