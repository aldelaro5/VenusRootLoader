using Microsoft.Extensions.Logging;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

using EventWithId = (int gameId, MethodInfo handler);

internal sealed class EventCollector : IBaseGameCollector
{
    private readonly ILeavesRegistry<EventLeaf> _eventsRegistry;
    private readonly ILogger<EventCollector> _logger;

    public EventCollector(ILeavesRegistry<EventLeaf> eventsRegistry, ILogger<EventCollector> logger)
    {
        _eventsRegistry = eventsRegistry;
        _logger = logger;
    }

    public void CollectBaseGameData()
    {
        List<EventWithId> eventsById = typeof(EventControl)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(x => x.Name.StartsWith("Event"))
            .Select(m => (gameId: int.Parse(m.Name[5..]), handler: m))
            .OrderBy(x => x.gameId)
            .ToList();

        foreach (EventWithId eventIdToMethod in eventsById)
        {
            EventLeaf eventLeaf = _eventsRegistry.RegisterExisting(
                eventIdToMethod.gameId,
                eventIdToMethod.gameId.ToString());
            eventLeaf.Handler =
                RootCollector.CreateOpenDelegateFromMethod<EventLeaf.EventHandler>(eventIdToMethod.handler);
        }

        RootCollector.LogCollectedAmount(_logger, _eventsRegistry, eventsById.Count);
    }
}