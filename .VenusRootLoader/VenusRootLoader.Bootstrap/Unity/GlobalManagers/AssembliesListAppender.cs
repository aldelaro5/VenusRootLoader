using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;

namespace VenusRootLoader.Bootstrap.Unity.GlobalManagers;

internal sealed class AssembliesListAppender : IHostedService
{
    private readonly ILogger<AssembliesListAppender> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IGlobalManagersPatchers _globalManagersHooks;

    private readonly HashSet<string> _assemblyNames = new();

    public AssembliesListAppender(
        ILogger<AssembliesListAppender> logger,
        IHostEnvironment hostEnvironment,
        IFileSystem fileSystem,
        IGlobalManagersPatchers globalManagersHooks)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _globalManagersHooks = globalManagersHooks;

        string budsDirectory = fileSystem.Path.Combine(hostEnvironment.ContentRootPath, "Buds");
        string venusRootLoaderDirectory = fileSystem.Path.Combine(hostEnvironment.ContentRootPath, "VenusRootLoader");
        AddAssemblyNamesFromDirectoryRecursively(fileSystem, venusRootLoaderDirectory);
        AddAssemblyNamesFromDirectoryRecursively(fileSystem, budsDirectory);
        _logger.LogTrace("\tFound the following assemblies:\n{assemblyNames}", string.Join('\n', _assemblyNames));
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
            try
            {
                AssemblyName.GetAssemblyName(dllOrExeFile);
                _assemblyNames.Add(_fileSystem.Path.GetFileName(dllOrExeFile));
            }
            catch (BadImageFormatException)
            {
                continue;
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _globalManagersHooks.RegisterPatcher(ShouldPatchAssembliesList, ChangeAssembliesList);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private bool ShouldPatchAssembliesList(
        AssetsManager manager,
        AssetsFileInstance globalManagersFileInstance,
        AssetsFile globalManagersFile)
    {
        _logger.LogDebug("\tReading MonoManager.m_AssemblyNames");
        AssetFileInfo monoManagerAsset = globalManagersFile.GetAssetInfo(6);
        AssetTypeValueField monoMangerBaseField = manager.GetBaseField(globalManagersFileInstance, monoManagerAsset);
        AssetTypeValueField assemblyNamesArray = monoMangerBaseField["m_AssemblyNames"][nameof(Array)];
        HashSet<string> additionalAssemblyNames = new();
        foreach (AssetTypeValueField assemblyNameField in assemblyNamesArray)
        {
            string assemblyName = assemblyNameField.AsString;
            if (assemblyName.StartsWith("UnityEngine") || assemblyName.StartsWith("Assembly-CSharp"))
                continue;
            additionalAssemblyNames.Add(assemblyName);
        }

        _logger.LogDebug(
            "\tRead the following assemblies:\n{assemblyNames}",
            string.Join("\n", additionalAssemblyNames));
        return !additionalAssemblyNames.SetEquals(_assemblyNames);
    }

    private void ChangeAssembliesList(
        AssetsManager manager,
        AssetsFileInstance globalManagersFileInstance,
        AssetsFile globalManagersFile)
    {
        _logger.LogDebug("\tAppending MonoManager.m_AssemblyNames");
        AssetFileInfo monoManagerAsset = globalManagersFile.GetAssetInfo(6);
        AssetTypeValueField monoMangerBaseField = manager.GetBaseField(globalManagersFileInstance, monoManagerAsset);
        AssetTypeValueField assemblyNamesArray = monoMangerBaseField["m_AssemblyNames"][nameof(Array)];
        foreach (string assemblyName in _assemblyNames)
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
}