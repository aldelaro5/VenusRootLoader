using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace VenusRootLoader.Bootstrap;

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
        Console.WriteLine("Using AssetTools.NET to create a modified game bundle that skips the splash screen...");

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
        using AssetsFileWriter writer = new(ModifiedGameBundlePath);
        bundleFile.Write(writer);

        // var newUncompressedBundle = new AssetBundleFile();
        // newUncompressedBundle.Read(new AssetsFileReader(File.OpenRead(uncompressedName)));
        // using (AssetsFileWriter writer = new AssetsFileWriter(ModifiedGameBundleFilename))
        // {
        //     newUncompressedBundle.Pack(writer, AssetBundleCompressionType.LZ4);
        // }

        Console.WriteLine("\tClosing the original bundle file");
        bundleFile.Close();

        Console.WriteLine($"Modified bundle file written successfully at {ModifiedGameBundlePath}");
    }
}