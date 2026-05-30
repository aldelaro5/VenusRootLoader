using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public abstract class ActionBehavior
{
    internal MapEntityLeaf MapEntityLeaf { get; }

    private ActionBehaviorKind? Kind { get; }

    internal NPCControl.ActionBehaviors InternalTypeForKind
    {
        get => Kind switch
        {
            ActionBehaviorKind.OutOfRange => MapEntityLeaf.InternalOutOfRangeBehavior,
            ActionBehaviorKind.InRange => MapEntityLeaf.InternalInRangeBehavior,
            null => NPCControl.ActionBehaviors.None,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(Kind))
        };
        set
        {
            switch (Kind)
            {
                case null:
                    return;
                case ActionBehaviorKind.OutOfRange:
                    MapEntityLeaf.InternalOutOfRangeBehavior = value;
                    break;
                case ActionBehaviorKind.InRange:
                    MapEntityLeaf.InternalInRangeBehavior = value;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(Kind));
                    break;
            }
        }
    }

    internal float InternalFrequencyForKind
    {
        get => Kind switch
        {
            ActionBehaviorKind.OutOfRange => MapEntityLeaf.InternalOutOfRangeActionFrequency,
            ActionBehaviorKind.InRange => MapEntityLeaf.InternalInRangeActionFrequency,
            null => 0f,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<float>(nameof(Kind))
        };
        set
        {
            switch (Kind)
            {
                case null:
                    return;
                case ActionBehaviorKind.OutOfRange:
                    MapEntityLeaf.InternalOutOfRangeActionFrequency = value;
                    break;
                case ActionBehaviorKind.InRange:
                    MapEntityLeaf.InternalInRangeActionFrequency = value;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(Kind));
                    break;
            }
        }
    }

    protected ActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind? kind)
    {
        MapEntityLeaf = mapEntityLeaf;
        Kind = kind;
    }
}