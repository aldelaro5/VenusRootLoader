using AssetsTools.NET;
using AssetsTools.NET.Extra;

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
internal static class UnitySplashScreenSkipper
{
    private static readonly string ModifiedGameBundlePath =
        Path.Combine(Entry.GameDir, "VenusRootLoader", "data.unity3d.modified");
    private static readonly string ClassDataTpkPath =
        Path.Combine(Entry.GameDir, "VenusRootLoader", "classdata.tpk");

    internal static void Setup()
    {
        FileHandleHook.RegisterHook(IsGameBundleFile, HookFileHandle);
    }

    private static bool IsGameBundleFile(string filename)
    {
        return filename.EndsWith("data.unity3d");
    }

    private static bool HookFileHandle(out nint originalHandle, string lpFilename, uint dwDesiredAccess, int dwShareMode, nint lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, nint hTemplateFile)
    {
        if (!File.Exists(ModifiedGameBundlePath))
            SetGameBundleToSkipSplashScreen(lpFilename);

        Console.WriteLine($"Redirecting game bundle to {ModifiedGameBundlePath}");
        originalHandle = WindowsNative.CreateFileW(ModifiedGameBundlePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        return true;
    }

    private static void SetGameBundleToSkipSplashScreen(string gameBundlePath)
    {
        Console.WriteLine("Using AssetTools.NET to create a modified game bundle that skips the Unity splash screen...");

        var manager = new AssetsManager();
        Console.WriteLine("\tLoading the classdata.tpk file");
        manager.LoadClassPackage(ClassDataTpkPath);

        Console.WriteLine("\tLoading the bundle file");
        var bundleInstance = manager.LoadBundleFile(gameBundlePath);
        var bundleFile = bundleInstance.file;

        Console.WriteLine("\tLoading the globalmanagers assets file");
        var assetsFileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, 0);
        var assetFile = assetsFileInstance.file;

        Console.WriteLine("\tLoading the class database using the assets file's Unity version");
        manager.LoadClassDatabaseFromPackage(assetFile.Metadata.UnityVersion);

        Console.WriteLine("\tSetting PlayerSettings.m_ShowUnitySplashScreen to false");
        var playerSettingAsset = assetFile.GetAssetInfo(1);
        var playerSettingsTypeValueField = manager.GetBaseField(assetsFileInstance, playerSettingAsset);
        playerSettingsTypeValueField["m_ShowUnitySplashScreen"].AsBool = false;
        playerSettingAsset.SetNewData(playerSettingsTypeValueField);

        Console.WriteLine("\tSetting BuildSettings.hasPROVersion to true");
        var buildSettingAsset = assetFile.GetAssetInfo(11);
        var buildSettingsTypeValueField = manager.GetBaseField(assetsFileInstance, buildSettingAsset);
        buildSettingsTypeValueField["hasPROVersion"].AsBool = true;
        buildSettingAsset.SetNewData(buildSettingsTypeValueField);

        Console.WriteLine("\tSetting the new globalmanagers data in the bundle");
        bundleFile.BlockAndDirInfo.DirectoryInfos[0].SetNewData(assetFile);

        Console.WriteLine("\tWriting the modified bundle file");
        using (AssetsFileWriter writer = new(ModifiedGameBundlePath))
            bundleFile.Write(writer);

        Console.WriteLine("\tClosing the original bundle file");
        bundleFile.Close();

        Console.WriteLine($"Modified bundle file written successfully at {ModifiedGameBundlePath}");
    }
}