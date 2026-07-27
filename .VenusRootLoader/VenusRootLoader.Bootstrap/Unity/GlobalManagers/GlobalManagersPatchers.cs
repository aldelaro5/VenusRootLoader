using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;

namespace VenusRootLoader.Bootstrap.Unity.GlobalManagers;

internal interface IGlobalManagersPatchers
{
    void RegisterPatcher(
        GlobalManagersPatchers.CreateBundlePredicate predicate,
        GlobalManagersPatchers.PatchGlobalManagers patcher);
}

/// <summary>
/// <para>
/// This service implements a way to skip the Unity splash screen that pops when the game window appears, but before the
/// game boots. It involves intercepting the opening of the game bundle (data.unity3d file) to create a modified version
/// using AssetTools.NET. The modified version will have 2 fields edited in the globalmanagers assets file which determine
/// if the splash screen should execute or not. The downside of this is the bundle needs to be saved on disk so we cache it
/// inside the VenusRootLoader folder to not consume more disk space for further boots.
/// </para>
/// </summary>
internal sealed class GlobalManagersPatchers : IGlobalManagersPatchers
{
    private static bool _hasModifiedBundle;

    public delegate bool CreateBundlePredicate(
        AssetsManager assetsManager,
        AssetsFileInstance globalManagersFileInstance,
        AssetsFile globalManagersFile);

    public delegate void PatchGlobalManagers(
        AssetsManager assetsManager,
        AssetsFileInstance globalManagersFileInstance,
        AssetsFile globalManagersFile);

    private readonly IFileSystem _fileSystem;
    private readonly string _modifiedGameBundlePath;
    private readonly string _classDataTpkPath;

    private readonly IWin32 _win32;
    private readonly ILogger<GlobalManagersPatchers> _logger;
    private readonly ICreateFileWSharedHooker _createFileWSharedHooker;
    private readonly GameExecutionContext _gameExecutionContext;

    private readonly List<(CreateBundlePredicate predicate, PatchGlobalManagers Patcher)> _globalManagersHooks = new();

    public unsafe GlobalManagersPatchers(
        ILogger<GlobalManagersPatchers> logger,
        ICreateFileWSharedHooker createFileWSharedHooker,
        GameExecutionContext gameExecutionContext,
        IWin32 win32,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
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
        _createFileWSharedHooker.RegisterHook(nameof(GlobalManagersPatchers), IsGameBundleFile, HookFileHandle);
    }

    public void RegisterPatcher(CreateBundlePredicate predicate, PatchGlobalManagers patcher) =>
        _globalManagersHooks.Add((predicate, patcher));

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
        bool modifiedBundleExists = _fileSystem.File.Exists(_modifiedGameBundlePath);
        List<bool> runPatchers = new();
        if (modifiedBundleExists)
        {
            _logger.LogDebug("\tLoading the modified bundle file");
            BundleFileInstance modifiedBundleInstance = manager.LoadBundleFile(_modifiedGameBundlePath);

            _logger.LogDebug("\tLoading the modified globalmanagers assets file");
            AssetsFileInstance modifiedAssetsFileInstance = manager.LoadAssetsFileFromBundle(modifiedBundleInstance, 0);
            AssetsFile modifiedAssetFile = modifiedAssetsFileInstance.file;

            LoadClassDatabase(manager, modifiedAssetFile);

            modifiedBundleVersion = ReadBundleVersion(manager, modifiedAssetsFileInstance, modifiedAssetFile);
            foreach ((CreateBundlePredicate predicate, PatchGlobalManagers Hook) patcher in _globalManagersHooks)
            {
                bool runPatcher = patcher.predicate(manager, modifiedAssetsFileInstance, modifiedAssetFile);
                runPatchers.Add(runPatcher);
            }

            manager.UnloadAll();
        }
        else
        {
            for (int i = 0; i < _globalManagersHooks.Count; i++)
                runPatchers.Add(true);
        }

        _logger.LogDebug("\tLoading the original bundle file");
        BundleFileInstance bundleInstance = manager.LoadBundleFile(originalGameBundlePath);
        AssetBundleFile bundleFile = bundleInstance.file;

        _logger.LogDebug("\tLoading the original globalmanagers assets file");
        AssetsFileInstance assetsFileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, 0);
        AssetsFile assetFile = assetsFileInstance.file;

        if (manager.ClassDatabase is null)
            LoadClassDatabase(manager, assetFile);

        bool bundleWasPatched = false;
        for (int i = 0; i < _globalManagersHooks.Count; i++)
        {
            (CreateBundlePredicate predicate, PatchGlobalManagers Hook) patcher = _globalManagersHooks[i];
            if (!runPatchers[i])
                continue;

            patcher.Hook(manager, assetsFileInstance, assetFile);
            bundleWasPatched = true;
        }

        string originalBundleVersion = ReadBundleVersion(manager, assetsFileInstance, assetFile);
        if (!bundleWasPatched && modifiedBundleExists && originalBundleVersion == modifiedBundleVersion)
        {
            _logger.LogDebug("\tThe modified bundle is already up to date, closing the AssetsManager");
            manager.UnloadAll(true);
            _hasModifiedBundle = true;
            return;
        }

        _logger.LogDebug("\tSetting the new globalmanagers data in the bundle");
        bundleFile.BlockAndDirInfo.DirectoryInfos[0].SetNewData(assetFile);

        _logger.LogDebug("\tWriting the modified bundle file");
        string uncompressedBundlePath = _modifiedGameBundlePath + ".uncompressed";
        using (AssetsFileWriter writer = new(uncompressedBundlePath))
            bundleFile.Write(writer);

        _logger.LogDebug("\tCompressing the modified bundle file...");
        AssetBundleFile newUncompressedBundle = new();
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

        _hasModifiedBundle = true;
    }

    private void LoadClassDatabase(AssetsManager manager, AssetsFile globalManagersAssetFile)
    {
        _logger.LogDebug("\tLoading the class database using the assets file's Unity version");
        manager.LoadClassDatabaseFromPackage(globalManagersAssetFile.Metadata.UnityVersion);
    }

    private string ReadBundleVersion(
        AssetsManager manager,
        AssetsFileInstance globalManagersInstance,
        AssetsFile globalManagersAssetFile)
    {
        _logger.LogDebug("\tReading PlayerSettings.bundleVersion");
        AssetFileInfo playerSettingAsset = globalManagersAssetFile.GetAssetInfo(1);
        AssetTypeValueField playerSettingsTypeValueField =
            manager.GetBaseField(globalManagersInstance, playerSettingAsset);

        string modifiedBundleVersion = playerSettingsTypeValueField["bundleVersion"].AsString;
        _logger.LogDebug("\tRead {bundleVersion}", modifiedBundleVersion);
        return modifiedBundleVersion;
    }
}