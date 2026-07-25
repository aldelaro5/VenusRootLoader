using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;
using VenusRootLoader.Api;

namespace VenusRootLoader.BudLoading;

/// <summary>
/// The service that oversees every phase of the bud loading process. It consists of the following phases in order:
/// <list type="number">
/// <item><see cref="IBudsDiscoverer"/>: Discovers all buds from the buds directory on disk.</item>
/// <item><see cref="IBudsValidator"/>: Removes all buds that aren't semantically valid from this point such as duplicates or incompatible buds.</item>
/// <item><see cref="IBudsDependencySorter"/>: Sorts all buds topologically given their depencencies.</item>
/// <item><see cref="IBudsLoadOrderEnumerator"/>: Enumerate all buds whose loading conditions have all been fulfilled.</item>
/// </list>
/// All the buds enumerated from the last phase gets loaded after handling their configuration file.
/// </summary>
internal sealed class BudLoader
{
    private readonly IBudsDiscoverer _budsDiscoverer;
    private readonly IBudsValidator _budsValidator;
    private readonly IBudsDependencySorter _budsDependencySorter;
    private readonly IBudsLoadOrderEnumerator _budsLoadOrderEnumerator;
    private readonly IAssemblyLoader _assemblyLoader;
    private readonly BudLoaderContext _budLoaderContext;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<BudLoader> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IVenusFactory _venusFactory;
    private readonly IBudConfigManager _budConfigManager;

    public BudLoader(
        IBudsDiscoverer budsDiscoverer,
        IBudsValidator budsValidator,
        IBudsDependencySorter budsDependencySorter,
        IBudsLoadOrderEnumerator budsLoadOrderEnumerator,
        IAssemblyLoader assemblyLoader,
        BudLoaderContext budLoaderContext,
        IFileSystem fileSystem,
        ILogger<BudLoader> logger,
        ILoggerFactory loggerFactory,
        IVenusFactory venusFactory,
        IBudConfigManager budConfigManager)
    {
        _budsDiscoverer = budsDiscoverer;
        _budsValidator = budsValidator;
        _budsDependencySorter = budsDependencySorter;
        _budsLoadOrderEnumerator = budsLoadOrderEnumerator;
        _assemblyLoader = assemblyLoader;
        _budLoaderContext = budLoaderContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _venusFactory = venusFactory;
        _budConfigManager = budConfigManager;

        if (!_fileSystem.Directory.Exists(_budLoaderContext.BudsPath))
            _fileSystem.Directory.CreateDirectory(_budLoaderContext.BudsPath);
    }

    internal void LoadAllBuds()
    {
        IList<BudInfo> buds = FindAllBuds();
        if (buds.Count == 0)
            return;

        IDictionary<string, BudInfo> budsById = _budsValidator.RemoveInvalidBuds(buds);
        IList<BudInfo> sortedBuds = DetermineBudsLoadOrder(budsById);
        if (sortedBuds.Count == 0)
            return;

        foreach (BudInfo budLoadingInfo in _budsLoadOrderEnumerator.EnumerateBudsWithFulfilledDependencies(sortedBuds))
            LoadBud(budLoadingInfo);
    }

    private IList<BudInfo> FindAllBuds()
    {
        try
        {
            return _budsDiscoverer.DiscoverAllBudsFromDisk();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while discovering buds");
            return [];
        }
    }

    private IList<BudInfo> DetermineBudsLoadOrder(IDictionary<string, BudInfo> buds)
    {
        try
        {
            IList<BudInfo> sortedMods = _budsDependencySorter.SortBudsTopologicallyFromDependencyGraph(buds);
            return sortedMods;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while determining the mods load order");
            return [];
        }
    }

    private void LoadBud(BudInfo budLoadingInfo)
    {
        try
        {
            Assembly assembly = _assemblyLoader.LoadFromPath(budLoadingInfo.BudAssemblyPath);
            Type budType = assembly.GetType(budLoadingInfo.BudType.FullName);
            Bud bud = (Bud)Activator.CreateInstance(budType);
            object? configData = UpdateConfig(bud, budLoadingInfo);
            bud.BudInfo = budLoadingInfo.BudManifest;
            bud.Logger = _loggerFactory.CreateLogger(budLoadingInfo.BudManifest.BudId);
            bud.BaseBudPath = _fileSystem.Path.GetDirectoryName(budLoadingInfo.BudAssemblyPath)!;
            bud.Venus = _venusFactory.CreateVenusForBud(budLoadingInfo.BudManifest.BudId);
            bud.ConfigData = configData;

            _logger.LogDebug("Loading bud {budId}...", budLoadingInfo.BudManifest.BudId);
            bud.Main();
            _logger.LogDebug("Loaded bud {budId} successfully", budLoadingInfo.BudManifest.BudId);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An exception occurred while loading the bud {budId}",
                budLoadingInfo.BudManifest.BudId);
            _budsLoadOrderEnumerator.MarkBudAsFailedDuringLoad(budLoadingInfo);
        }
    }

    private object? UpdateConfig(Bud bud, BudInfo budLoadingInfo)
    {
        object? defaultConfigData = bud.DefaultConfigData;
        if (defaultConfigData is null)
            return null;

        Type configType = defaultConfigData.GetType();
        string configPath = _budConfigManager.GetConfigPathForBud(budLoadingInfo.BudManifest.BudId);
        if (_fileSystem.File.Exists(configPath))
        {
            object o = _budConfigManager.Load(budLoadingInfo.BudManifest.BudId, configType);
            _budConfigManager.Save(budLoadingInfo.BudManifest.BudId, configType, o, defaultConfigData);
            return o;
        }

        _budConfigManager.Save(budLoadingInfo.BudManifest.BudId, configType, defaultConfigData, defaultConfigData);
        return defaultConfigData;
    }
}