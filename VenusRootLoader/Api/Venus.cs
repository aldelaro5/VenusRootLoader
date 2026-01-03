using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public sealed partial class Venus
{
    private readonly string _budId;
    private readonly ILeavesRegistry<ItemLeaf, int> _itemsRegistry;
    private readonly GlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ILogger<Venus> _logger;

    internal Venus(
        string budId,
        ILeavesRegistry<ItemLeaf, int> itemsRegistry,
        GlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ILogger<Venus> logger)
    {
        _budId = budId;
        _itemsRegistry = itemsRegistry;
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _logger = logger;
    }
}