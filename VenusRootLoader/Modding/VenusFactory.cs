using Microsoft.Extensions.Logging;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Modding;

internal interface IVenusFactory
{
    Venus CreateVenusForBud(string budId);
}

internal sealed class VenusFactory : IVenusFactory
{
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    public VenusFactory(
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger)
    {
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
    }

    public Venus CreateVenusForBud(string budId) => new(
        budId,
        new()
        {
            GlobalMonoBehaviourExecution = _globalMonoBehaviourExecution,
            Logger = _logger
        });
}