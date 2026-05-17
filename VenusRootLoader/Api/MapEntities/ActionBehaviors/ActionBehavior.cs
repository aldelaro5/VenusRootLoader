using CommunityToolkit.Diagnostics;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public abstract class ActionBehavior
{
    internal abstract NPCControl.ActionBehaviors BehaviorType { get; }
    internal MapEntity MapEntity { get; }
    internal ActionBehaviorKind Kind { get; }

    internal float InternalFrequency
    {
        get => Kind switch
        {
            ActionBehaviorKind.OutOfRange => MapEntity.InternalPrimaryActionFrequency,
            ActionBehaviorKind.InRange => MapEntity.InternalSecondaryActionFrequency,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<float>(nameof(Kind))
        };
        set
        {
            switch (Kind)
            {
                case ActionBehaviorKind.OutOfRange:
                    MapEntity.InternalPrimaryActionFrequency = value;
                    break;
                case ActionBehaviorKind.InRange:
                    MapEntity.InternalSecondaryActionFrequency = value;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException<float>(nameof(Kind));
                    break;
            }
        }
    }

    protected ActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind)
    {
        MapEntity = mapEntity;
        Kind = kind;
    }

    internal abstract void InitializeFromExisting(IRegistryResolver registryResolver);
}