using Microsoft.Extensions.Logging;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public sealed partial class Venus
{
    internal readonly string _budId;
    internal readonly IRegistryResolver _registryResolver;
    internal readonly IGlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    internal readonly ILogger<Venus> _logger;

    internal Venus(
        string budId,
        IRegistryResolver registryResolver,
        IGlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger)
    {
        _budId = budId;
        _registryResolver = registryResolver;
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
    }
}