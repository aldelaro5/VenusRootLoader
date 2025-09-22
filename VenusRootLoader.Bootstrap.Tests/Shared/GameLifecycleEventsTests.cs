using AwesomeAssertions;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Tests.Shared;

public class GameLifecycleEventsTests
{
    [Fact]
    public void Publish_CallsAllSubscribers_WhenThereAreSubscribers()
    {
        bool firstCalled = false;
        bool secondCalled = false;
        bool thirdCalled = false;

        var sut = new GameLifecycleEvents();
        sut.Subscribe((_, _) => firstCalled = true);
        sut.Subscribe((_, _) => secondCalled = true);
        sut.Subscribe((_, _) => thirdCalled = true);

        sut.Publish(this, new GameLifecycleEventArgs { LifeCycle = GameLifecycle.MonoInitialising });

        firstCalled.Should().BeTrue();
        secondCalled.Should().BeTrue();
        thirdCalled.Should().BeTrue();
    }
}