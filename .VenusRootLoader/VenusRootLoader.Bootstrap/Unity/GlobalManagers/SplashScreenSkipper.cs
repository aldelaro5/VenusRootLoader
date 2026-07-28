using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings;

namespace VenusRootLoader.Bootstrap.Unity.GlobalManagers;

internal sealed class SplashScreenSkipper : IHostedService
{
    private readonly ILogger<SplashScreenSkipper> _logger;
    private readonly IGlobalManagersPatchers _globalManagersHooks;

    private readonly bool _enableSkipper;

    public SplashScreenSkipper(
        ILogger<SplashScreenSkipper> logger,
        IOptions<GlobalSettings> globalSettings,
        IGlobalManagersPatchers globalManagersHooks)
    {
        _logger = logger;
        _globalManagersHooks = globalManagersHooks;
        _enableSkipper = globalSettings.Value.SkipUnitySplashScreen!.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _globalManagersHooks.RegisterPatcher(ShouldSkipSplashScreen, EditSplashScreenSettings);
        return Task.CompletedTask;
    }

    private bool ShouldSkipSplashScreen(
        AssetsManager assetsManager,
        AssetsFileInstance globalManagersFileInstance)
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

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void EditSplashScreenSettings(
        AssetsManager manager,
        AssetsFileInstance globalManagersFileInstance)
    {
        _logger.LogDebug("\tSetting PlayerSettings.m_ShowUnitySplashScreen to {value}", !_enableSkipper);
        AssetFileInfo playerSettingAsset = globalManagersFileInstance.file.GetAssetInfo(1);
        AssetTypeValueField playerSettingsTypeValueField =
            manager.GetBaseField(globalManagersFileInstance, playerSettingAsset);
        playerSettingsTypeValueField["m_ShowUnitySplashScreen"].AsBool = !_enableSkipper;
        playerSettingAsset.SetNewData(playerSettingsTypeValueField);

        _logger.LogDebug("\tSetting BuildSettings.hasPROVersion to {value}", _enableSkipper);
        AssetFileInfo buildSettingAsset = globalManagersFileInstance.file.GetAssetInfo(11);
        AssetTypeValueField buildSettingsTypeValueField =
            manager.GetBaseField(globalManagersFileInstance, buildSettingAsset);
        buildSettingsTypeValueField["hasPROVersion"].AsBool = _enableSkipper;
        buildSettingAsset.SetNewData(buildSettingsTypeValueField);
    }
}