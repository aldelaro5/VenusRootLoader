using Microsoft.Extensions.Logging;
using VenusRootLoader.VenusInternals;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public sealed partial class Venus
{
    private readonly string _budId;
    private readonly ContentRegistry _contentRegistry;
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    internal Venus(
        string budId,
        ContentRegistry contentRegistry,
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger)
    {
        _budId = budId;
        _contentRegistry = contentRegistry;
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
    }
}