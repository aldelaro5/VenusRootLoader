using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class ChasePlayerMapEntityBehavior : MapEntityBehavior
{
    public bool ChaseOnWater
    {
        get => InternalTypeForKind switch
        {
            NPCControl.ActionBehaviors.ChasePlayer => false,
            NPCControl.ActionBehaviors.ChaseOnWater => true,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>(nameof(InternalTypeForKind))
        };
        set => InternalTypeForKind = value
            ? NPCControl.ActionBehaviors.ChaseOnWater
            : NPCControl.ActionBehaviors.ChasePlayer;
    }

    public float MovementSpeedMultiplier
    {
        get => MapEntityLeaf.InternalSpeedMultiplier;
        set => MapEntityLeaf.InternalSpeedMultiplier = value;
    }

    internal ChasePlayerMapEntityBehavior(MapEntityLeaf mapEntityLeaf, BehaviorKind kind) : base(mapEntityLeaf, kind)
    {
    }
}