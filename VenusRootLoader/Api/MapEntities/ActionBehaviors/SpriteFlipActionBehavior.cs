using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class SpriteFlipActionBehavior : ActionBehavior
{
    public bool FlipsAtRandomInterval
    {
        get => InternalTypeForKind switch
        {
            NPCControl.ActionBehaviors.TurnFixedInterval => false,
            NPCControl.ActionBehaviors.TurnRandomly => true,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>(nameof(InternalTypeForKind))
        };
        set => InternalTypeForKind = value
            ? NPCControl.ActionBehaviors.TurnRandomly
            : NPCControl.ActionBehaviors.TurnFixedInterval;
    }

    public float BaseFlipIntervalInFrames
    {
        get => InternalFrequencyForKind;
        set => InternalFrequencyForKind = value;
    }

    internal SpriteFlipActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(mapEntity, kind)
    {
    }
}