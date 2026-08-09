using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class EventCollector : IBaseGameCollector
{
    private readonly IAssemblyCSharpDataCollector _assemblyCSharpDataCollector;
    private readonly ILeavesRegistry<EventLeaf> _eventsRegistry;
    private readonly ILogger<EventCollector> _logger;

    public EventCollector(
        IAssemblyCSharpDataCollector assemblyCSharpDataCollector,
        ILeavesRegistry<EventLeaf> eventsRegistry,
        ILogger<EventCollector> logger)
    {
        _assemblyCSharpDataCollector = assemblyCSharpDataCollector;
        _eventsRegistry = eventsRegistry;
        _logger = logger;
    }

    public void CollectBaseGameData()
    {
        IList<int> eventIds = _assemblyCSharpDataCollector.GetEventControlEventsIds();
        foreach (int eventId in eventIds)
            _eventsRegistry.RegisterExisting(eventId, eventId.ToString());

        RootCollector.LogCollectedAmount(_logger, _eventsRegistry, eventIds.Count);
    }
}