using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public abstract class MapEntityBehavior
{
    internal MapEntityLeaf MapEntityLeaf { get; }

    private BehaviorKind? Kind { get; }

    internal NPCControl.ActionBehaviors InternalTypeForKind
    {
        get => Kind switch
        {
            BehaviorKind.OutOfRange => MapEntityLeaf.InternalOutOfRangeBehavior,
            BehaviorKind.InRange => MapEntityLeaf.InternalInRangeBehavior,
            null => NPCControl.ActionBehaviors.None,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(Kind))
        };
        set
        {
            switch (Kind)
            {
                case null:
                    return;
                case BehaviorKind.OutOfRange:
                    MapEntityLeaf.InternalOutOfRangeBehavior = value;
                    break;
                case BehaviorKind.InRange:
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
            BehaviorKind.OutOfRange => MapEntityLeaf.InternalOutOfRangeActionFrequency,
            BehaviorKind.InRange => MapEntityLeaf.InternalInRangeActionFrequency,
            null => 0f,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<float>(nameof(Kind))
        };
        set
        {
            switch (Kind)
            {
                case null:
                    return;
                case BehaviorKind.OutOfRange:
                    MapEntityLeaf.InternalOutOfRangeActionFrequency = value;
                    break;
                case BehaviorKind.InRange:
                    MapEntityLeaf.InternalInRangeActionFrequency = value;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(Kind));
                    break;
            }
        }
    }

    protected MapEntityBehavior(MapEntityLeaf mapEntityLeaf, BehaviorKind? kind)
    {
        MapEntityLeaf = mapEntityLeaf;
        Kind = kind;
    }
}