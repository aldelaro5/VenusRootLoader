using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class NegatableMapEntityActivation
{
    internal readonly Ref<int> IntRef = new(0);

    public required Branch<ObjectMapEntityLeaf> MapEntity
    {
        get;
        set
        {
            field = value;
            IntRef.Value = EffectiveValue;
        }
    }

    public required bool IsActivationValueNegated
    {
        get;
        set
        {
            field = value;
            if (MapEntity.Leaf is not null)
                IntRef.Value = EffectiveValue;
        }
    }

    internal int EffectiveValue => MapEntity.GameId * (IsActivationValueNegated ? -1 : 1);
}