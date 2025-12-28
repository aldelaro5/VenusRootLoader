using Microsoft.Extensions.Logging;
using VenusRootLoader.Public;

namespace VenusRootLoader.VenusInternals;

internal interface IVenusFactory
{
    Venus CreateVenusForBud(string budId);
}

internal sealed class VenusFactory : IVenusFactory
{
    private readonly ContentRegistry _contentRegistry;
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    public VenusFactory(
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger,
        ContentRegistry contentRegistry)
    {
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
        _contentRegistry = contentRegistry;
    }

    public Venus CreateVenusForBud(string budId) => new(
        budId,
        _contentRegistry,
        _globalMonoBehaviourExecution,
        _logger);
}