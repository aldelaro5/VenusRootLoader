namespace VenusRootLoader.Bootstrap.Shared;

public interface IMonoInitLifeCycleEvents
{
    void Subscribe(EventHandler listener);
    void Publish(object sender);
}

/// <summary>
/// This service is used to inform its subscriber services that Mono has initialised.
/// This is a key moment in the bootstrap because it implies a lot of hooks can be cleaned up
/// as they are no longer needed.
/// </summary>
public sealed class MonoInitLifeCycleEvents : IMonoInitLifeCycleEvents
{
    private readonly List<EventHandler> _events = new();

    public void Subscribe(EventHandler listener) => _events.Add(listener);

    public void Publish(object sender)
    {
        foreach (var listener in _events)
            listener(sender, EventArgs.Empty);
    }
}