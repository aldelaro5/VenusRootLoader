using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;
using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal class ModLoader : IHostedService
{
    private readonly IModsDiscoverer _modsDiscoverer;
    private readonly IModsDependencySorter _modsDependencySorter;
    private readonly IModsLoadOrderEnumerator _modsLoadOrderEnumerator;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ModLoader> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    public ModLoader(
        IModsDiscoverer modsDiscoverer,
        IModsDependencySorter modsDependencySorter,
        IModsLoadOrderEnumerator modsLoadOrderEnumerator,
        IFileSystem fileSystem,
        ILogger<ModLoader> logger,
        ILoggerFactory loggerFactory)
    {
        _modsDiscoverer = modsDiscoverer;
        _modsDependencySorter = modsDependencySorter;
        _modsLoadOrderEnumerator = modsLoadOrderEnumerator;
        _fileSystem = fileSystem;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        IList<ModInfo> mods = DiscoverMods();
        IEnumerable<ModInfo> sortedMods = SortModsInLoadOrder(mods);

        foreach (ModInfo modLoadingInfo in _modsLoadOrderEnumerator.EnumerateModsWithFulfilledDependencies(sortedMods))
            LoadMod(modLoadingInfo);

        return Task.CompletedTask;
    }

    private IList<ModInfo> DiscoverMods()
    {
        try
        {
            return _modsDiscoverer.DiscoverAllMods();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while loading mods");
            return [];
        }
    }

    private IList<ModInfo> SortModsInLoadOrder(IList<ModInfo> mods)
    {
        try
        {
            IList<ModInfo> sortedMods = _modsDependencySorter.DetermineModsLoadOrder(mods);
            _logger.LogInformation(string.Join(" -> ", sortedMods.Select(x => x.ModManifest.ModId)));
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
            Assembly assembly = Assembly.LoadFrom(modLoadingInfo.ModAssemblyPath);
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
            _modsLoadOrderEnumerator.MarkModAsFailed(modLoadingInfo);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}