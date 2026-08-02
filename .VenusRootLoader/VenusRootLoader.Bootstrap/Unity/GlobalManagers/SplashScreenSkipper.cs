using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings;

namespace VenusRootLoader.Bootstrap.Unity.GlobalManagers;

/// <summary>
/// <para>
/// This service implements a way to skip the Unity splash screen that pops when the game window appears, but before the
/// game boots. It involves intercepting the opening of the game bundle (data.unity3d file) to create a modified version
/// using AssetTools.NET. The modified version will have 2 fields edited in the globalmanagers assets file which determine
/// if the splash screen should execute or not. The downside of this is the bundle needs to be saved on disk so we cache it
/// inside the VenusRootLoader folder to not consume more disk space for further boots.
/// </para>
/// </summary>
internal sealed class SplashScreenSkipper : IGlobalManagersPatcher
{
    private readonly ILogger<SplashScreenSkipper> _logger;

    private readonly bool _enableSkipper;

    public SplashScreenSkipper(
        ILogger<SplashScreenSkipper> logger,
        IOptions<GlobalSettings> globalSettings)
    {
        _logger = logger;
        _enableSkipper = globalSettings.Value.SkipUnitySplashScreen!.Value;
    }

    public bool ShouldPatch(AssetsManager assetsManager, AssetsFileInstance globalManagersFileInstance)
    {
        _logger.LogDebug("\tReading PlayerSettings.m_ShowUnitySplashScreen");
        AssetFileInfo playerSettingAsset = globalManagersFileInstance.file.GetAssetInfo(1);
        AssetTypeValueField playerSettingsTypeValueField =
            assetsManager.GetBaseField(globalManagersFileInstance, playerSettingAsset);
        bool showSplashScreen = playerSettingsTypeValueField["m_ShowUnitySplashScreen"].AsBool;
        bool shouldSkipSplashScreen = (showSplashScreen && _enableSkipper) || (!showSplashScreen && !_enableSkipper);
        if (shouldSkipSplashScreen)
            _logger.LogDebug($"\tWill be patching");
        return shouldSkipSplashScreen;
    }

    public void Patch(AssetsManager assetsManager, AssetsFileInstance globalManagersFileInstance)
    {
        _logger.LogDebug("\tSetting PlayerSettings.m_ShowUnitySplashScreen to {value}", !_enableSkipper);
        AssetFileInfo playerSettingAsset = globalManagersFileInstance.file.GetAssetInfo(1);
        AssetTypeValueField playerSettingsTypeValueField =
            assetsManager.GetBaseField(globalManagersFileInstance, playerSettingAsset);
        playerSettingsTypeValueField["m_ShowUnitySplashScreen"].AsBool = !_enableSkipper;
        playerSettingAsset.SetNewData(playerSettingsTypeValueField);

        _logger.LogDebug("\tSetting BuildSettings.hasPROVersion to {value}", _enableSkipper);
        AssetFileInfo buildSettingAsset = globalManagersFileInstance.file.GetAssetInfo(11);
        AssetTypeValueField buildSettingsTypeValueField =
            assetsManager.GetBaseField(globalManagersFileInstance, buildSettingAsset);
        buildSettingsTypeValueField["hasPROVersion"].AsBool = _enableSkipper;
        buildSettingAsset.SetNewData(buildSettingsTypeValueField);
    }
}