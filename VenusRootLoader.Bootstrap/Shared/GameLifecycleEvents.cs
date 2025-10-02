namespace VenusRootLoader.Bootstrap.Shared;

public interface IGameLifecycleEvents
{
    void Subscribe(EventHandler listener);
    void Publish(object sender);
}

public class GameLifecycleEvents : IGameLifecycleEvents
{
    private readonly List<EventHandler> _events = new();

    public void Subscribe(EventHandler listener) => _events.Add(listener);

    public void Publish(object sender)
    {
        foreach (var listener in _events)
            listener(sender, EventArgs.Empty);
    }
}