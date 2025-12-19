using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;
using VenusRootLoader.Modding;
using VenusRootLoader.Models;

namespace VenusRootLoader.BudLoading;

internal sealed class BudLoader : IHostedService
{
    private readonly IBudsDiscoverer _budsDiscoverer;
    private readonly IBudsValidator _budsValidator;
    private readonly IBudsDependencySorter _budsDependencySorter;
    private readonly IBudsLoadOrderEnumerator _budsLoadOrderEnumerator;
    private readonly IAssemblyLoader _assemblyLoader;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<BudLoader> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IVenusFactory _venusFactory;

    public BudLoader(
        IBudsDiscoverer budsDiscoverer,
        IBudsValidator budsValidator,
        IBudsDependencySorter budsDependencySorter,
        IBudsLoadOrderEnumerator budsLoadOrderEnumerator,
        IAssemblyLoader assemblyLoader,
        IFileSystem fileSystem,
        ILogger<BudLoader> logger,
        ILoggerFactory loggerFactory,
        IVenusFactory venusFactory)
    {
        _budsDiscoverer = budsDiscoverer;
        _budsValidator = budsValidator;
        _budsDependencySorter = budsDependencySorter;
        _budsLoadOrderEnumerator = budsLoadOrderEnumerator;
        _assemblyLoader = assemblyLoader;
        _fileSystem = fileSystem;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _venusFactory = venusFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        IList<BudInfo> buds = FindAllBuds();
        IDictionary<string, BudInfo> budsById = _budsValidator.RemoveInvalidBuds(buds);
        IList<BudInfo> sortedBuds = DetermineBudsLoadOrder(budsById);

        foreach (BudInfo budLoadingInfo in _budsLoadOrderEnumerator.EnumerateBudsWithFulfilledDependencies(sortedBuds))
            LoadBud(budLoadingInfo);

        return Task.CompletedTask;
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
            bud.Logger = _loggerFactory.CreateLogger(budLoadingInfo.BudManifest.BudId);
            bud.BaseBudPath = _fileSystem.Path.GetDirectoryName(budLoadingInfo.BudAssemblyPath)!;
            bud.Venus = _venusFactory.CreateVenusForBud(budLoadingInfo.BudManifest.BudId);

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

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}