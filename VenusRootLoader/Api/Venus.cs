using Microsoft.Extensions.Logging;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public sealed partial class Venus
{
    private readonly string _budId;
    private readonly RegistryResolver _registryResolver;
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    internal Venus(
        string budId,
        RegistryResolver registryResolver,
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger)
    {
        _budId = budId;
        _registryResolver = registryResolver;
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
    }
}