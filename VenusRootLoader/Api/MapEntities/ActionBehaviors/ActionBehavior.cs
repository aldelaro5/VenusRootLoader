using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public abstract class ActionBehavior
{
    internal MapEntity MapEntity { get; }

    private ActionBehaviorKind? Kind { get; }

    internal NPCControl.ActionBehaviors InternalTypeForKind
    {
        get => Kind switch
        {
            ActionBehaviorKind.OutOfRange => MapEntity.InternalPrimaryBehavior,
            ActionBehaviorKind.InRange => MapEntity.InternalSecondaryBehavior,
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
                    MapEntity.InternalPrimaryBehavior = value;
                    break;
                case ActionBehaviorKind.InRange:
                    MapEntity.InternalSecondaryBehavior = value;
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
            ActionBehaviorKind.OutOfRange => MapEntity.InternalPrimaryActionFrequency,
            ActionBehaviorKind.InRange => MapEntity.InternalSecondaryActionFrequency,
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
                    MapEntity.InternalPrimaryActionFrequency = value;
                    break;
                case ActionBehaviorKind.InRange:
                    MapEntity.InternalSecondaryActionFrequency = value;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(Kind));
                    break;
            }
        }
    }

    protected ActionBehavior(MapEntity mapEntity, ActionBehaviorKind? kind)
    {
        MapEntity = mapEntity;
        Kind = kind;
    }
}