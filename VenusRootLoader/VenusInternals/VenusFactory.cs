using Microsoft.Extensions.Logging;
using VenusRootLoader.Public;

namespace VenusRootLoader.VenusInternals;

internal interface IVenusFactory
{
    Venus CreateVenusForBud(string budId);
}

internal sealed class VenusFactory : IVenusFactory
{
    private readonly ContentBinder _contentBinder;
    private readonly GlobalContentRegistry _globalContentRegistry;
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    public VenusFactory(
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger,
        GlobalContentRegistry globalContentRegistry,
        ContentBinder contentBinder)
    {
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
        _globalContentRegistry = globalContentRegistry;
        _contentBinder = contentBinder;
    }

    public Venus CreateVenusForBud(string budId) => new(
        budId,
        new()
        {
            ContentBinder = _contentBinder,
            GlobalContentRegistry = _globalContentRegistry,
            GlobalMonoBehaviourExecution = _globalMonoBehaviourExecution,
            Logger = _logger
        });
}