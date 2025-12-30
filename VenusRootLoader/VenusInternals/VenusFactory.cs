using Microsoft.Extensions.Logging;
using Venus = VenusRootLoader.Api.Venus;

namespace VenusRootLoader.VenusInternals;

internal interface IVenusFactory
{
    Venus CreateVenusForBud(string budId);
}

internal sealed class VenusFactory : IVenusFactory
{
    private readonly LeavesRegistry _leavesRegistry;
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    public VenusFactory(
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger,
        LeavesRegistry leavesRegistry)
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