using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;

namespace VenusRootLoader.Bootstrap.Unity.GlobalManagers;

internal sealed class RootGlobalManagersPatcher
{
    private static bool _hasModifiedBundle;

    private readonly IFileSystem _fileSystem;
    private readonly string _modifiedGameBundlePath;
    private readonly string _classDataTpkPath;

    private readonly IWin32 _win32;
    private readonly ILogger<RootGlobalManagersPatcher> _logger;
    private readonly ICreateFileWSharedHooker _createFileWSharedHooker;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly List<IGlobalManagersPatcher> _globalManagersPatchers;

    public RootGlobalManagersPatcher(
        IEnumerable<IGlobalManagersPatcher> globalManagersPatchers,
        ILogger<RootGlobalManagersPatcher> logger,
        ICreateFileWSharedHooker createFileWSharedHooker,
        GameExecutionContext gameExecutionContext,
        IWin32 win32,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _globalManagersPatchers = globalManagersPatchers.ToList();
        _gameExecutionContext = gameExecutionContext;
        _win32 = win32;
        _createFileWSharedHooker = createFileWSharedHooker;
        _modifiedGameBundlePath = _fileSystem.Path.Combine(
            _gameExecutionContext.GameDir,
            "VenusRootLoader",
            "data.unity3d.modified");
        _classDataTpkPath = _fileSystem.Path.Combine(
            _gameExecutionContext.GameDir,
            "VenusRootLoader",
            "classdata.tpk");
    }

    public unsafe void SetupPatchers()
    {
        _createFileWSharedHooker.RegisterHook(nameof(RootGlobalManagersPatcher), IsGameBundleFile, HookFileHandle);
    }

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
        if (!_hasModifiedBundle)
        {
            try
            {
                EditGameBundle(lpFileName.ToString());
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

    private void EditGameBundle(string originalGameBundlePath)
    {
        _logger.LogInformation(
            "Using AssetTools.NET to create a modified game bundle using the one from {gameBundlePath}...",
            originalGameBundlePath);

        AssetsManager manager = new() { UseQuickLookup = true };
        _logger.LogDebug("\tLoading the classdata.tpk file");
        manager.LoadClassPackage(_classDataTpkPath);

        string modifiedBundleVersion = "";
        AssetsFileInstance? modifiedAssetsFileInstance = null;
        List<bool> runPatchers = new();
        if (_fileSystem.File.Exists(_modifiedGameBundlePath))
        {
            _logger.LogDebug("\tLoading the modified bundle file");
            BundleFileInstance modifiedBundleInstance = manager.LoadBundleFile(_modifiedGameBundlePath);

            _logger.LogDebug("\tLoading the modified globalmanagers assets file");
            modifiedAssetsFileInstance = manager.LoadAssetsFileFromBundle(modifiedBundleInstance, 0);

            LoadClassDatabase(manager, modifiedAssetsFileInstance.file);

            modifiedBundleVersion = ReadBundleVersion(manager, modifiedAssetsFileInstance);
            foreach (IGlobalManagersPatcher patcher in _globalManagersPatchers)
            {
                bool runPatcher = patcher.ShouldPatch(manager, modifiedAssetsFileInstance);
                runPatchers.Add(runPatcher);
            }
        }
        else
        {
            for (int i = 0; i < _globalManagersPatchers.Count; i++)
                runPatchers.Add(true);
        }

        _logger.LogDebug("\tLoading the original bundle file");
        BundleFileInstance bundleInstance = manager.LoadBundleFile(originalGameBundlePath);
        AssetBundleFile bundleFile = bundleInstance.file;

        _logger.LogDebug("\tLoading the original globalmanagers assets file");
        AssetsFileInstance originalAssetsFileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, 0);
        AssetsFile originalAssetFile = originalAssetsFileInstance.file;

        if (manager.ClassDatabase is null)
            LoadClassDatabase(manager, originalAssetFile);

        string originalBundleVersion = ReadBundleVersion(manager, originalAssetsFileInstance);
        bool patchesNeedsToRun = runPatchers.Any(x => x);
        if (!patchesNeedsToRun
            && modifiedAssetsFileInstance is not null
            && originalBundleVersion == modifiedBundleVersion)
        {
            _logger.LogDebug("\tThe modified bundle is already up to date, closing the AssetsManager");
            manager.UnloadAll(true);
            _hasModifiedBundle = true;
            return;
        }

        AssetsFileInstance assetsFileInstance = patchesNeedsToRun && modifiedAssetsFileInstance is not null
            ? modifiedAssetsFileInstance
            : originalAssetsFileInstance;
        for (int i = 0; i < _globalManagersPatchers.Count; i++)
        {
            IGlobalManagersPatcher patcher = _globalManagersPatchers[i];
            if (!runPatchers[i])
                continue;

            patcher.Patch(manager, assetsFileInstance);
        }

        GenerateModifiedGameBundle(manager, bundleFile, assetsFileInstance, _modifiedGameBundlePath);

        _logger.LogDebug("\tClosing the AssetsManager");
        manager.UnloadAll(true);

        _logger.LogInformation(
            "Modified bundle file written successfully at {ModifiedGameBundlePath}",
            _modifiedGameBundlePath);

        _hasModifiedBundle = true;
    }

    private void GenerateModifiedGameBundle(
        AssetsManager manager,
        AssetBundleFile originalBundleFile,
        AssetsFileInstance newGlobalManagersInstance,
        string modifiedGameBundlePath)
    {
        _logger.LogDebug("\tSetting the new globalmanagers data in the bundle");
        originalBundleFile.BlockAndDirInfo.DirectoryInfos[0].SetNewData(newGlobalManagersInstance.file);

        _logger.LogDebug("\tWriting the modified bundle file");
        string uncompressedBundlePath = _modifiedGameBundlePath + ".uncompressed";
        using (AssetsFileWriter writer = new(uncompressedBundlePath))
            originalBundleFile.Write(writer);

        _logger.LogDebug("\tClosing the modified bundle file (if it exists)...");
        manager.UnloadBundleFile(modifiedGameBundlePath);

        _logger.LogDebug("\tCompressing the modified bundle file...");
        AssetBundleFile newUncompressedBundle = new();
        newUncompressedBundle.Read(new AssetsFileReader(_fileSystem.File.OpenRead(uncompressedBundlePath)));
        using (AssetsFileWriter writer = new(_modifiedGameBundlePath))
            newUncompressedBundle.Pack(writer, AssetBundleCompressionType.LZ4Fast);
        newUncompressedBundle.Close();

        _fileSystem.File.Delete(uncompressedBundlePath);
    }

    private void LoadClassDatabase(AssetsManager manager, AssetsFile globalManagersAssetFile)
    {
        _logger.LogDebug("\tLoading the class database using the assets file's Unity version");
        manager.LoadClassDatabaseFromPackage(globalManagersAssetFile.Metadata.UnityVersion);
    }

    private string ReadBundleVersion(
        AssetsManager manager,
        AssetsFileInstance globalManagersInstance)
    {
        _logger.LogDebug("\tReading PlayerSettings.bundleVersion");
        AssetFileInfo playerSettingAsset = globalManagersInstance.file.GetAssetInfo(1);
        AssetTypeValueField playerSettingsTypeValueField =
            manager.GetBaseField(globalManagersInstance, playerSettingAsset);

        string modifiedBundleVersion = playerSettingsTypeValueField["bundleVersion"].AsString;
        _logger.LogDebug("\tRead {bundleVersion}", modifiedBundleVersion);
        return modifiedBundleVersion;
    }
}