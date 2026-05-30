using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

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

    internal SpriteFlipActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) : base(mapEntityLeaf, kind)
    {
    }
}