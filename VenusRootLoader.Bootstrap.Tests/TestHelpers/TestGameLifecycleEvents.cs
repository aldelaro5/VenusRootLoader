using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Tests.TestHelpers;

public class TestGameLifecycleEvents : IGameLifecycleEvents
{
    internal List<EventHandler<GameLifecycleEventArgs>> Listeners { get; } = new();
    public void Subscribe(EventHandler<GameLifecycleEventArgs> listener) => Listeners.Add(listener);
    public void Publish(object sender, GameLifecycleEventArgs eventArgs)
    {
        foreach (var listener in Listeners)
            listener.Invoke(sender, eventArgs);
    }
}