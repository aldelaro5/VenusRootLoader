using System.Drawing;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// <para>
/// This class implements a way to skip the Unity splash screen that pops when the game window appears, but before the
/// game boots. It involves intercepting the opening of the game bundle (data.unity3d file) to create a modified version
/// using AssetTools.NET. The modified version will have 2 fields edited in the globalmanagers assets file which determine
/// if the splash screen should execute or not. The downside of this is the bundle needs to be saved on disk so we cache it
/// inside the VenusRootLoader folder to not consume more disk space for further boots.
/// </para>
/// <para>
/// The modified bundle file is saved uncompressed for performance reasons since compressing the bundle saves ~400 MB,
/// but takes ~13 seconds to do so which doesn't seem to be a worthy tradeoff
/// </para>
/// </summary>
internal class UnitySplashScreenSkipper : IHostedService
{
    private static readonly string ModifiedGameBundlePath =
        Path.Combine(Entry.GameDir, "VenusRootLoader", "data.unity3d.modified");
    private static readonly string ClassDataTpkPath =
        Path.Combine(Entry.GameDir, "VenusRootLoader", "classdata.tpk");

    private readonly ILogger _logger;

    public UnitySplashScreenSkipper(ILoggerFactory loggerFactory) =>
        _logger = loggerFactory.CreateLogger(nameof(UnitySplashScreenSkipper), Color.Magenta);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        FileHandleHook.RegisterHook(IsGameBundleFile, HookFileHandle);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static bool IsGameBundleFile(string filename) => filename.EndsWith("data.unity3d");

    private bool HookFileHandle(out nint originalHandle, string lpFilename, uint dwDesiredAccess, int dwShareMode, nint lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, nint hTemplateFile)
    {
        if (!File.Exists(ModifiedGameBundlePath))
            SetGameBundleToSkipSplashScreen(lpFilename);

        _logger.LogInformation("Redirecting game bundle to {ModifiedGameBundlePath}", ModifiedGameBundlePath);
        originalHandle = WindowsNative.CreateFileW(ModifiedGameBundlePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        return true;
    }

    private void SetGameBundleToSkipSplashScreen(string gameBundlePath)
    {
        _logger.LogInformation("Using AssetTools.NET to create a modified game bundle that skips the Unity splash screen...");

        var manager = new AssetsManager();
        _logger.LogDebug("\tLoading the classdata.tpk file");
        manager.LoadClassPackage(ClassDataTpkPath);

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
        using (AssetsFileWriter writer = new(ModifiedGameBundlePath))
            bundleFile.Write(writer);

        _logger.LogDebug("\tClosing the original bundle file");
        bundleFile.Close();

        _logger.LogInformation("Modified bundle file written successfully at {ModifiedGameBundlePath}", ModifiedGameBundlePath);
    }
}