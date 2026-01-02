using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Unity;
using Venus = VenusRootLoader.Api.Venus;

namespace VenusRootLoader.VenusInternals;

internal interface IVenusFactory
{
    Venus CreateVenusForBud(string budId);
}

internal sealed class VenusFactory : IVenusFactory
{
    private readonly ILeavesRegistry<ItemLeaf, int> _leavesRegistry;
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    public VenusFactory(
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger,
        ILeavesRegistry<ItemLeaf, int> leavesRegistry)
    {
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
        _leavesRegistry = leavesRegistry;
    }

    public Venus CreateVenusForBud(string budId) => new(
        budId,
        _leavesRegistry,
        _globalMonoBehaviourExecution,
        _logger);
}