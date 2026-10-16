using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class ListTypesCollector : IBaseGameCollector
{
    private readonly ILogger<ListTypesCollector> _logger;
    private readonly ILeavesRegistry<ListTypeLeaf> _listTypesRegistry;

    public ListTypesCollector(
        ILogger<ListTypesCollector> logger,
        ILeavesRegistry<ListTypeLeaf> listTypesRegistry)
    {
        _logger = logger;
        _listTypesRegistry = listTypesRegistry;
    }

    public void CollectBaseGameData()
    {
        Dictionary<int, MethodInfo> listTypeSetupHandlers = RootCollector.GetLeavesMethods(
                typeof(MainManager),
                nameof(MainManager.ShowItemList),
                "Setup")
            .ToDictionary(x => int.Parse(x.Key.Replace('m', '-'), CultureInfo.InvariantCulture), x => x.Value);
        Dictionary<int, MethodInfo> listTypeRenderers = RootCollector.GetLeavesMethods(
                typeof(MainManager),
                nameof(MainManager.ShowItemList),
                "Render")
            .ToDictionary(x => int.Parse(x.Key.Replace('m', '-'), CultureInfo.InvariantCulture), x => x.Value);

        foreach (int gameId in listTypeSetupHandlers.Keys.OrderBy(x => x))
        {
            ListTypeLeaf listTypeLeaf = _listTypesRegistry.RegisterExisting(
                gameId,
                gameId.ToString(CultureInfo.InvariantCulture));
            if (listTypeSetupHandlers.TryGetValue(gameId, out MethodInfo methodSetup))
            {
                listTypeLeaf.SetupHandler =
                    RootCollector.CreateOpenDelegateFromMethod<ListTypeLeaf.ListTypeSetupHandler>(methodSetup);
            }

            if (listTypeRenderers.TryGetValue(gameId, out MethodInfo methodRenderer))
            {
                listTypeLeaf.Renderer =
                    RootCollector.CreateOpenDelegateFromMethod<ListTypeLeaf.ListTypeRenderer>(methodRenderer);
            }
        }

        RootCollector.LogCollectedAmount(_logger, _listTypesRegistry, listTypeSetupHandlers.Count);
    }
}