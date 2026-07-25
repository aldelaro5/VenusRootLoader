using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;

namespace VenusRootLoader.Bootstrap.Unity;

/// <summary>
/// <para>
/// This service implements a way to skip the Unity splash screen that pops when the game window appears, but before the
/// game boots. It involves intercepting the opening of the game bundle (data.unity3d file) to create a modified version
/// using AssetTools.NET. The modified version will have 2 fields edited in the globalmanagers assets file which determine
/// if the splash screen should execute or not. The downside of this is the bundle needs to be saved on disk so we cache it
/// inside the VenusRootLoader folder to not consume more disk space for further boots.
/// </para>
/// </summary>
internal sealed class SplashScreenSkipper : IHostedService
{
    private readonly IFileSystem _fileSystem;
    private readonly string _modifiedGameBundlePath;
    private readonly string _classDataTpkPath;

    private readonly IWin32 _win32;
    private readonly ILogger _logger;
    private readonly ICreateFileWSharedHooker _createFileWSharedHooker;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly bool _enableSkipper;

    public SplashScreenSkipper(
        ILogger<SplashScreenSkipper> logger,
        ICreateFileWSharedHooker createFileWSharedHooker,
        GameExecutionContext gameExecutionContext,
        IOptions<GlobalSettings> globalSettings,
        IWin32 win32,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _gameExecutionContext = gameExecutionContext;
        _win32 = win32;
        _createFileWSharedHooker = createFileWSharedHooker;
        _enableSkipper = globalSettings.Value.SkipUnitySplashScreen!.Value;
        _modifiedGameBundlePath = _fileSystem.Path.Combine(
            _gameExecutionContext.GameDir,
            "VenusRootLoader",
            "data.unity3d.modified");
        _classDataTpkPath = _fileSystem.Path.Combine(
            _gameExecutionContext.GameDir,
            "VenusRootLoader",
            "classdata.tpk");
    }

    public unsafe Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_enableSkipper)
            return Task.CompletedTask;

        _createFileWSharedHooker.RegisterHook(nameof(SplashScreenSkipper), IsGameBundleFile, HookFileHandle);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private bool IsGameBundleFile(string filename) =>
        filename == _fileSystem.Path.Combine(_gameExecutionContext.DataDir, "data.unity3d");

    private unsafe void HookFileHandle(
        out HANDLE originalHandle,
        PCWSTR lpFileName,
        uint dwDesiredAccess,
        FILE_SHARE_MODE dwShareMode,
        SECURITY_ATTRIBUTES* lpSecurityAttributes,
        FILE_CREATION_DISPOSITION dwCreationDisposition,
        FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes,
        HANDLE hTemplateFile)
    {
        if (!_fileSystem.File.Exists(_modifiedGameBundlePath))
        {
            try
            {
                SetGameBundleToSkipSplashScreen(lpFileName.ToString());
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "An error occured while saving the modified bundle\n");
                throw;
            }
        }

        _logger.LogInformation("Redirecting game bundle to {ModifiedGameBundlePath}", _modifiedGameBundlePath);
        fixed (char* fileNamePtr = _modifiedGameBundlePath)
        {
            originalHandle = _win32.CreateFile(
                fileNamePtr,
                dwDesiredAccess,
                dwShareMode,
                new(lpSecurityAttributes),
                dwCreationDisposition,
                dwFlagsAndAttributes,
                hTemplateFile);
        }
    }

    private void SetGameBundleToSkipSplashScreen(string gameBundlePath)
    {
        _logger.LogInformation(
            "Using AssetTools.NET to create a modified game bundle that skips the Unity splash screen...");

        var manager = new AssetsManager();
        _logger.LogDebug("\tLoading the classdata.tpk file");
        manager.LoadClassPackage(_classDataTpkPath);

        _logger.LogDebug("\tLoading the bundle file");
        var bundleInstance = manager.LoadBundleFile(gameBundlePath);
        var bundleFile = bundleInstance.file;

        _logger.LogDebug("\tLoading the globalmanagers assets file");
        var assetsFileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, 0);
        var assetFile = assetsFileInstance.file;

        _logger.LogDebug("\tLoading the class database using the assets file's Unity version");
        manager.LoadClassDatabaseFromPackage(assetFile.Metadata.UnityVersion);

        _logger.LogDebug("\tSetting PlayerSettings.m_ShowUnitySplashScreen to false");
        var playerSettingAsset = assetFile.GetAssetInfo(1);
        var playerSettingsTypeValueField = manager.GetBaseField(assetsFileInstance, playerSettingAsset);
        playerSettingsTypeValueField["m_ShowUnitySplashScreen"].AsBool = false;
        playerSettingAsset.SetNewData(playerSettingsTypeValueField);

        _logger.LogDebug("\tSetting BuildSettings.hasPROVersion to true");
        var buildSettingAsset = assetFile.GetAssetInfo(11);
        var buildSettingsTypeValueField = manager.GetBaseField(assetsFileInstance, buildSettingAsset);
        buildSettingsTypeValueField["hasPROVersion"].AsBool = true;
        buildSettingAsset.SetNewData(buildSettingsTypeValueField);

        _logger.LogDebug("\tSetting the new globalmanagers data in the bundle");
        bundleFile.BlockAndDirInfo.DirectoryInfos[0].SetNewData(assetFile);

        _logger.LogDebug("\tWriting the modified bundle file");
        string uncompressedBundlePath = _modifiedGameBundlePath + ".uncompressed";
        using (AssetsFileWriter writer = new(uncompressedBundlePath))
            bundleFile.Write(writer);

        _logger.LogDebug("\tCompressing the modified bundle file...");
        var newUncompressedBundle = new AssetBundleFile();
        newUncompressedBundle.Read(new AssetsFileReader(_fileSystem.File.OpenRead(uncompressedBundlePath)));
        using (AssetsFileWriter writer = new(_modifiedGameBundlePath))
            newUncompressedBundle.Pack(writer, AssetBundleCompressionType.LZ4Fast);
        newUncompressedBundle.Close();

        _fileSystem.File.Delete(uncompressedBundlePath);

        _logger.LogDebug("\tClosing the AssetsManager");
        manager.UnloadAll();

        _logger.LogInformation(
            "Modified bundle file written successfully at {ModifiedGameBundlePath}",
            _modifiedGameBundlePath);
    }
}