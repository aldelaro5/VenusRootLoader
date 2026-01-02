using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public sealed partial class Venus
{
    private readonly string _budId;
    private readonly ILeavesRegistry<ItemLeaf, int> _leavesRegistry;
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    internal Venus(
        string budId,
        ILeavesRegistry<ItemLeaf, int> leavesRegistry,
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger)
    {
        _budId = budId;
        _leavesRegistry = leavesRegistry;
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
    }
}