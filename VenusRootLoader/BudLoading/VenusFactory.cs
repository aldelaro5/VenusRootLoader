using Microsoft.Extensions.Logging;
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
    private readonly IRegistryResolver _registryResolver;
    private readonly IGlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    public VenusFactory(
        IGlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        IRegistryResolver registryResolver,
        ILogger<Venus> logger)
    {
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
        _registryResolver = registryResolver;
    }

    public Venus CreateVenusForBud(string budId) => new(
        budId,
        _registryResolver,
        _globalMonoBehaviourExecution,
        _logger);
}