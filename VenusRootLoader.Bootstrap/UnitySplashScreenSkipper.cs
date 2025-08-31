using System.Runtime.InteropServices;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Win32.SafeHandles;

namespace VenusRootLoader.Bootstrap;

internal static class UnitySplashScreenSkipper
{
    private const uint BufferSize = 200_000_000;

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

    private static unsafe bool HookFileHandle(out nint originalHandle, string lpFilename, uint dwDesiredAccess, int dwShareMode, nint lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, nint hTemplateFile)
    {
        originalHandle = WindowsNative.CreateFileW(lpFilename, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        if (!File.Exists(ModifiedGameBundlePath))
        {
            uint bytesRead = 0;
            var buffer = Marshal.AllocHGlobal((int)BufferSize);
            WindowsNative.ReadFile(originalHandle, (void*)buffer, BufferSize, &bytesRead, nint.Zero);
            byte[] readBytes = new byte[(int)bytesRead];
            Marshal.Copy(buffer, readBytes, 0, (int)bytesRead);
            Console.WriteLine($"Read {bytesRead} bytes");
            WindowsNative.SetFilePointerEx(originalHandle, 0, null, 0);
            SetGameBundleToSkipSplashScreen(new(originalHandle, true));
        }

        WindowsNative.CloseHandle(originalHandle);
        Console.WriteLine($"Redirecting game bundle to {ModifiedGameBundlePath}");
        originalHandle = WindowsNative.CreateFileW(ModifiedGameBundlePath, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        return true;
    }

    private static void SetGameBundleToSkipSplashScreen(SafeFileHandle handle)
    {
        try
        {
            var manager = new AssetsManager();
            manager.LoadClassPackage(ClassDataTpkPath);
            FileStream fs = new FileStream(handle, FileAccess.ReadWrite);
            var bundleInstance = manager.LoadBundleFile(fs);
            var bundleFile = bundleInstance.file;
            var assetsFileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, 0);
            var assetFile = assetsFileInstance.file;
            manager.LoadClassDatabaseFromPackage(assetFile.Metadata.UnityVersion);
            
            var playerSettingAsset = assetFile.GetAssetInfo(1);
            var playerSettingsTypeValueField = manager.GetBaseField(assetsFileInstance, playerSettingAsset);
            playerSettingsTypeValueField["m_ShowUnitySplashScreen"].AsBool = false;
            playerSettingAsset.SetNewData(playerSettingsTypeValueField);
        
            var buildSettingAsset = assetFile.GetAssetInfo(11);
            var buildSettingsTypeValueField = manager.GetBaseField(assetsFileInstance, buildSettingAsset);
            buildSettingsTypeValueField["hasPROVersion"].AsBool = true;
            buildSettingAsset.SetNewData(buildSettingsTypeValueField);
        
            bundleFile.BlockAndDirInfo.DirectoryInfos[0].SetNewData(assetFile);

            using AssetsFileWriter writer = new(ModifiedGameBundlePath);
            bundleFile.Write(writer);

            // var newUncompressedBundle = new AssetBundleFile();
            // newUncompressedBundle.Read(new AssetsFileReader(File.OpenRead(uncompressedName)));
            // using (AssetsFileWriter writer = new AssetsFileWriter(ModifiedGameBundleFilename))
            // {
            //     newUncompressedBundle.Pack(writer, AssetBundleCompressionType.LZ4);
            // }
        }
        catch (Exception e)
        {
            WindowsNative.MessageBoxA(nint.Zero, e.ToString(), "Exception", 0x30);
            throw;
        }
    }
}