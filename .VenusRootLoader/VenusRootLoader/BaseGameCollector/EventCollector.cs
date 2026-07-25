using AsmResolver.DotNet;
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

    public void CollectBaseGameData(string baseGameId)
    {
        Dictionary<int, MethodDefinition> eventMethods = _assemblyCSharpDataCollector.GetEventControlEvents();
        foreach (KeyValuePair<int, MethodDefinition> kvpEventMethod in eventMethods)
        {
            MethodDefinition eventMethod = eventMethods[kvpEventMethod.Key];
            _eventsRegistry.RegisterExisting(kvpEventMethod.Key, eventMethod.Name!, baseGameId);
        }

        _logger.LogInformation(
            "Collected and registered {EventsAmount} base game Events",
            eventMethods.Count);
    }
}