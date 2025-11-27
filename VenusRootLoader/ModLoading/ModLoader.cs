using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;
using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal sealed class ModLoader : IHostedService
{
    private readonly IModsDiscoverer _modsDiscoverer;
    private readonly IModsValidator _modsValidator;
    private readonly IModsDependencySorter _modsDependencySorter;
    private readonly IModsLoadOrderEnumerator _modsLoadOrderEnumerator;
    private readonly IAssemblyLoader _assemblyLoader;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ModLoader> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    public ModLoader(
        IModsDiscoverer modsDiscoverer,
        IModsValidator modsValidator,
        IModsDependencySorter modsDependencySorter,
        IModsLoadOrderEnumerator modsLoadOrderEnumerator,
        IAssemblyLoader assemblyLoader,
        IFileSystem fileSystem,
        ILogger<ModLoader> logger,
        ILoggerFactory loggerFactory)
    {
        _modsDiscoverer = modsDiscoverer;
        _modsValidator = modsValidator;
        _modsDependencySorter = modsDependencySorter;
        _modsLoadOrderEnumerator = modsLoadOrderEnumerator;
        _assemblyLoader = assemblyLoader;
        _fileSystem = fileSystem;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        IList<ModInfo> mods = FindAllMods();
        IDictionary<string, ModInfo> modsById = _modsValidator.RemoveInvalidMods(mods);
        IList<ModInfo> sortedMods = DetermineModsLoadOrder(modsById);

        foreach (ModInfo modLoadingInfo in _modsLoadOrderEnumerator.EnumerateModsWithFulfilledDependencies(sortedMods))
            LoadMod(modLoadingInfo);

        return Task.CompletedTask;
    }

    private IList<ModInfo> FindAllMods()
    {
        try
        {
            return _modsDiscoverer.DiscoverAllModsFromDisk();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while discovering mods");
            return [];
        }
    }

    private IList<ModInfo> DetermineModsLoadOrder(IDictionary<string, ModInfo> mods)
    {
        try
        {
            IList<ModInfo> sortedMods = _modsDependencySorter.SortModsTopologicallyFromDependencyGraph(mods);
            return sortedMods;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while determining the mods load order");
            return [];
        }
    }

    private void LoadMod(ModInfo modLoadingInfo)
    {
        try
        {
            Assembly assembly = _assemblyLoader.LoadFromPath(modLoadingInfo.ModAssemblyPath);
            Type modType = assembly.GetType(modLoadingInfo.ModType.FullName);
            Mod mod = (Mod)Activator.CreateInstance(modType);
            mod.Logger = _loggerFactory.CreateLogger(modLoadingInfo.ModManifest.ModId);
            mod.BaseModPath = _fileSystem.Path.GetDirectoryName(modLoadingInfo.ModAssemblyPath)!;

            _logger.LogDebug("Loading mod {modId}...", modLoadingInfo.ModManifest.ModId);
            mod.Main();
            _logger.LogDebug("Loaded mod {modId} successfully", modLoadingInfo.ModManifest.ModId);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An exception occurred while loading the mod {modId}",
                modLoadingInfo.ModManifest.ModId);
            _modsLoadOrderEnumerator.MarkModAsFailedDuringLoad(modLoadingInfo);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}