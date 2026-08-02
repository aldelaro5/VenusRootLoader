using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.InteropServices;
using VenusRootLoader.Bootstrap.Mono;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;

namespace VenusRootLoader.Bootstrap.Unity.GlobalManagers;

/// <summary>
/// This service aims to solve an issue where when a Unity Object is defined in a custom assembly.
/// If the Object contains a field whose type is a collection or a type with a [Serializable] attribute, it will be
/// deserialized to null. This happens because Unity will only consider assemblies present in the MonoManager's m_AssemblyNames
/// for deserializing. This list is baked into the globalmanagers asset from the game's asset bundle (data.unity3d).
/// As an added barrier, even if the assembly were to be in that list, Unity will assume its presence in the game's Managed
/// folder because it assumes all the assemblies it needs will be present there when the game was built. This service
/// aims to solve this issue by eagerly adding all the potential assemblies that could contain Unity Objects. More specifically,
/// it means it will add VenusRootLoader.Unity.Runtime.dll and all .NET assemblies found under the Buds directory recursively.
/// <p>
/// The bundle redirect works the same as other globalmanagers patchers, but we need to convince Unity that the assemblies
/// exists under the Managed directory which requires a series of hooks:
/// <list type="bullet">
/// <item>PltHook PathFileExistsW so Unity thinks the assemblies exists in the directory.</item>
/// <item>Install a CreateFileW hook so Unity reads the redirected assemblies.</item>
/// <item>PltHook GetFileAttributesExW so Unity obtains the actual properties of the redirected assemblies files, notably their file sizes.</item>
/// <item>Redirect mono_image_open_from_data_with_name so Mono loads them using their redirected paths (this is handled by <see cref="MonoInitializer"/>).</item>
/// </list>
/// </p>
/// The service exposes the assemblies list to append for the <see cref="MonoInitializer"/> to do the last step mentioned above.
/// </summary>
internal interface IAssembliesListAppender
{
    /// <summary>
    /// Called when mono_image_open_from_data_with_name happens to redirect an assembly's path from its original.
    /// </summary>
    /// <param name="originalName">The original assembly path.</param>
    /// <returns>The redirected assembly path</returns>
    string OnMonoImageOpenFromDataWithName(string originalName);
}

/// <inheritdoc cref="IAssembliesListAppender"/>
internal sealed class AssembliesListAppender : IGlobalManagersPatcher, IAssembliesListAppender
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private delegate BOOL PathFileExistsFn(PCWSTR pszPath);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private unsafe delegate BOOL GetFileAttributesExWFn(
        PCWSTR lpFileName,
        GET_FILEEX_INFO_LEVELS fInfoLevelId,
        void* lpFileInformation);

    private const string VenusRootLoaderUnityRuntimeFilename = "VenusRootLoader.Unity.Runtime.dll";

    private static PathFileExistsFn _hookPathFileExistsDelegate = null!;
    private static GetFileAttributesExWFn _hookGetFileAttributesExDelegate = null!;
    private static string _managedDirectoryPath = string.Empty;

    private readonly ILogger<AssembliesListAppender> _logger;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly IWin32 _win32;
    private readonly IFileSystem _fileSystem;
    private readonly IPltHooksManager _pltHooksManager;
    private readonly ICreateFileWSharedHooker _createFileWSharedHooker;

    private readonly Dictionary<string, string> _assemblyNames = new();

    public unsafe AssembliesListAppender(
        ILogger<AssembliesListAppender> logger,
        GameExecutionContext gameExecutionContext,
        IWin32 win32,
        IFileSystem fileSystem,
        IPltHooksManager pltHooksManager,
        ICreateFileWSharedHooker createFileWSharedHooker)
    {
        _logger = logger;
        _gameExecutionContext = gameExecutionContext;
        _fileSystem = fileSystem;
        _pltHooksManager = pltHooksManager;
        _createFileWSharedHooker = createFileWSharedHooker;
        _win32 = win32;
        _managedDirectoryPath = _fileSystem.Path.Combine(_gameExecutionContext.DataDir, "Managed");

        PopulateAssembliesList(gameExecutionContext, fileSystem);

        _hookPathFileExistsDelegate = HookPathFileExistsW;
        _hookGetFileAttributesExDelegate = HookGetFileAttributesEx;
        _pltHooksManager.InstallHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "PathFileExistsW",
            _hookPathFileExistsDelegate);
        _pltHooksManager.InstallHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "GetFileAttributesExW",
            _hookGetFileAttributesExDelegate);
        _createFileWSharedHooker.RegisterHook(nameof(AssembliesListAppender), IsCustomAssemblyFile, HookFileHandle);
    }

    public bool ShouldPatch(AssetsManager assetsManager, AssetsFileInstance globalManagersFileInstance)
    {
        _logger.LogDebug("\tReading MonoManager.m_AssemblyNames");
        AssetFileInfo monoManagerAsset = globalManagersFileInstance.file.GetAssetInfo(6);
        AssetTypeValueField monoMangerBaseField =
            assetsManager.GetBaseField(globalManagersFileInstance, monoManagerAsset);
        AssetTypeValueField assemblyNamesArray = monoMangerBaseField["m_AssemblyNames"][nameof(Array)];
        HashSet<string> additionalAssemblyNames = new();
        foreach (AssetTypeValueField assemblyNameField in assemblyNamesArray)
        {
            string assemblyName = assemblyNameField.AsString;
            if (assemblyName.StartsWith("UnityEngine") || assemblyName.StartsWith("Assembly-CSharp"))
                continue;
            additionalAssemblyNames.Add(assemblyName);
        }

        _logger.LogTrace(
            "\tRead the following assemblies:\n{assemblyNames}",
            string.Join("\n", additionalAssemblyNames));
        bool shouldPatchAssembliesList = !additionalAssemblyNames.SetEquals(_assemblyNames.Keys.ToHashSet());
        if (shouldPatchAssembliesList)
            _logger.LogDebug("\tWill be patching");
        return shouldPatchAssembliesList;
    }

    public void Patch(AssetsManager assetsManager, AssetsFileInstance globalManagersFileInstance)
    {
        _logger.LogDebug("\tAppending MonoManager.m_AssemblyNames");
        AssetFileInfo monoManagerAsset = globalManagersFileInstance.file.GetAssetInfo(6);
        AssetTypeValueField monoMangerBaseField =
            assetsManager.GetBaseField(globalManagersFileInstance, monoManagerAsset);
        AssetTypeValueField assemblyNamesArray = monoMangerBaseField["m_AssemblyNames"][nameof(Array)];

        HashSet<string> allAssemblyNames = new();
        foreach (AssetTypeValueField assemblyNameField in assemblyNamesArray)
        {
            string assemblyName = assemblyNameField.AsString;
            if (assemblyName.StartsWith("UnityEngine") || assemblyName.StartsWith("Assembly-CSharp"))
                allAssemblyNames.Add(assemblyName);
        }

        foreach (string assemblyName in _assemblyNames.Keys)
            allAssemblyNames.Add(assemblyName);

        assemblyNamesArray.Children.Clear();
        foreach (string assemblyName in allAssemblyNames)
        {
            AssetTypeValueField newArrayItem = ValueBuilder.DefaultValueFieldFromArrayTemplate(assemblyNamesArray);
            newArrayItem.AsString = assemblyName;
            assemblyNamesArray.Children.Add(newArrayItem);
        }

        _logger.LogTrace(
            "\tWriting the following assemblies:\n{assemblyNames}",
            string.Join("\n", assemblyNamesArray.Children.Select(x => x.AsString)));
        monoManagerAsset.SetNewData(monoMangerBaseField);
    }

    private void PopulateAssembliesList(GameExecutionContext gameExecutionContext, IFileSystem fileSystem)
    {
        string budsDirectory = fileSystem.Path.Combine(gameExecutionContext.BaseDir, "Buds");
        string venusRootLoaderDirectory = fileSystem.Path.Combine(
            gameExecutionContext.BaseDir,
            "VenusRootLoader");

        // We want all buds assemblies except the ones we already have so we take priority over them.
        AddAssemblyNamesFromDirectoryRecursively(fileSystem, budsDirectory);
        RemoveAssemblyNamesFromDirectoryRecursively(fileSystem, venusRootLoaderDirectory);

        // Add the VenusRootLoader.Unity.Runtime.dll as an exception to the above since it's our assembly expected
        // to contain our Unity Objects we want to deserialize.
        string venusRootLoaderUnityRuntimeAssemblyPath = fileSystem.Path.Combine(
            venusRootLoaderDirectory,
            VenusRootLoaderUnityRuntimeFilename);
        _assemblyNames.Add(VenusRootLoaderUnityRuntimeFilename, venusRootLoaderUnityRuntimeAssemblyPath);

        _logger.LogTrace(
            "\tFound the following assemblies:\n{assemblyNames}",
            string.Join('\n', _assemblyNames.Select(x => $"{x.Key}: {x.Value}")));
    }

    private void AddAssemblyNamesFromDirectoryRecursively(IFileSystem fileSystem, string budsDirectory)
    {
        IEnumerable<string> dllFiles = fileSystem.Directory.EnumerateFiles(
            budsDirectory,
            "*.dll",
            SearchOption.AllDirectories);
        IEnumerable<string> exeFiles = fileSystem.Directory.EnumerateFiles(
            budsDirectory,
            "*.exe",
            SearchOption.AllDirectories);
        foreach (string dllOrExeFile in dllFiles.Concat(exeFiles))
        {
            string fileName = _fileSystem.Path.GetFileName(dllOrExeFile);
            if (_assemblyNames.ContainsKey(fileName))
                continue;

            // The try catch flow is required, this is the best way to safely check the file is a .NET assembly
            // without adding it to the domain if they are.
            try
            {
                AssemblyName.GetAssemblyName(dllOrExeFile);
                _assemblyNames.Add(fileName, dllOrExeFile);
            }
            catch (BadImageFormatException)
            {
                continue;
            }
        }
    }

    private void RemoveAssemblyNamesFromDirectoryRecursively(IFileSystem fileSystem, string budsDirectory)
    {
        IEnumerable<string> dllFiles = fileSystem.Directory.EnumerateFiles(
            budsDirectory,
            "*.dll",
            SearchOption.AllDirectories);
        IEnumerable<string> exeFiles = fileSystem.Directory.EnumerateFiles(
            budsDirectory,
            "*.exe",
            SearchOption.AllDirectories);
        foreach (string dllOrExeFile in dllFiles.Concat(exeFiles))
        {
            string fileName = _fileSystem.Path.GetFileName(dllOrExeFile);
            _assemblyNames.Remove(fileName);
        }
    }

    private BOOL HookPathFileExistsW(PCWSTR pszPath)
    {
        if (!pszPath.AsSpan().StartsWith(_managedDirectoryPath))
            return _win32.PathFileExists(pszPath);

        string path = pszPath.ToString();
        string fileName = _fileSystem.Path.GetFileName(path);
        if (_assemblyNames.ContainsKey(fileName))
            return true;

        return _win32.PathFileExists(pszPath);
    }

    private bool IsCustomAssemblyFile(string path)
    {
        if (!path.StartsWith(_managedDirectoryPath))
            return false;

        string fileName = _fileSystem.Path.GetFileName(path);
        return _assemblyNames.ContainsKey(fileName);
    }

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
        string path = lpFileName.ToString();
        string fileName = _fileSystem.Path.GetFileName(path);
        string redirectedPath = _assemblyNames[fileName];

        _logger.LogTrace(
            "Redirecting {originalPath} Unity's Managed assembly to {redirectedPath}",
            path,
            redirectedPath);
        fixed (char* fileNamePtr = redirectedPath)
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

    private unsafe BOOL HookGetFileAttributesEx(
        PCWSTR lpFileName,
        GET_FILEEX_INFO_LEVELS fInfoLevelId,
        void* lpFileInformation)
    {
        if (!lpFileName.AsSpan().StartsWith(_managedDirectoryPath))
            return _win32.GetFileAttributesExW(lpFileName, fInfoLevelId, lpFileInformation);

        string path = lpFileName.ToString();
        string fileName = _fileSystem.Path.GetFileName(path);
        if (!_assemblyNames.TryGetValue(fileName, out string? redirectedPath))
            return _win32.GetFileAttributesExW(lpFileName, fInfoLevelId, lpFileInformation);

        _logger.LogTrace(
            "Redirecting {originalPath} Unity's Managed file attributes assembly to {redirectedPath}",
            path,
            redirectedPath);
        fixed (char* fileNamePtr = redirectedPath)
            return _win32.GetFileAttributesExW(fileNamePtr, fInfoLevelId, lpFileInformation);
    }

    public string OnMonoImageOpenFromDataWithName(string originalName)
    {
        if (!originalName.StartsWith(_managedDirectoryPath))
            return originalName;

        string fileName = _fileSystem.Path.GetFileName(originalName);
        if (!_assemblyNames.Remove(fileName, out string? redirectedPath))
            return originalName;

        if (_assemblyNames.Count == 0)
        {
            _pltHooksManager.UninstallHook(_gameExecutionContext.UnityPlayerDllFileName, "PathFileExistsW");
            _pltHooksManager.UninstallHook(_gameExecutionContext.UnityPlayerDllFileName, "GetFileAttributesExW");
            _createFileWSharedHooker.UnregisterHook(nameof(AssembliesListAppender));
        }

        _logger.LogDebug("Redirecting the image load of {original} to {redirectedPath}", originalName, redirectedPath);
        return redirectedPath;
    }
}