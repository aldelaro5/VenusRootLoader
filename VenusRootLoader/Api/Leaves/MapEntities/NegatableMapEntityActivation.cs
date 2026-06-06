using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities;

public sealed class NegatableMapEntityActivation
{
    internal readonly Ref<int> Ref = new(0);

    public required Branch<MapEntityLeaf> MapEntity
    {
        get;
        set
        {
            field = value;
            Ref.Value = EffectiveValue;
        }
    }

    public required bool IsActivationValueNegated
    {
        get;
        set
        {
            field = value;
            Ref.Value = EffectiveValue;
        }
    }

    internal int EffectiveValue => MapEntity.GameId * (IsActivationValueNegated ? -1 : 1);
}