namespace VenusRootLoader.Bootstrap.Shared;

public enum GameLifecycle
{
    MonoInitialising
}

public class GameLifecycleEventArgs : EventArgs
{
    public GameLifecycle LifeCycle { get; init; }
}

public interface IGameLifecycleEvents
{
    void Subscribe(EventHandler<GameLifecycleEventArgs> listener);
    void Publish(object sender, GameLifecycleEventArgs eventArgs);
}

public class GameLifecycleEvents : IGameLifecycleEvents
{
    private readonly List<EventHandler<GameLifecycleEventArgs>> _events = new();

    public void Subscribe(EventHandler<GameLifecycleEventArgs> listener) => _events.Add(listener);

    public void Publish(object sender, GameLifecycleEventArgs eventArgs)
    {
        foreach (var listener in _events)
            listener(sender, eventArgs);
    }
}