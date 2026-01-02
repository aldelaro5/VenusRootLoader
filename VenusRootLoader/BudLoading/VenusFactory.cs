using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;
using Venus = VenusRootLoader.Api.Venus;

namespace VenusRootLoader.BudLoading;

internal interface IVenusFactory
{
    Venus CreateVenusForBud(string budId);
}

internal sealed class VenusFactory : IVenusFactory
{
    private readonly ILeavesRegistry<ItemLeaf, int> _itemsRegistry;
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    public VenusFactory(
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger,
        ILeavesRegistry<ItemLeaf, int> itemsRegistry)
    {
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
        _itemsRegistry = itemsRegistry;
    }

    public Venus CreateVenusForBud(string budId) => new(
        budId,
        _itemsRegistry,
        _globalMonoBehaviourExecution,
        _logger);
}