namespace VenusRootLoader.Api.Leaves.MapEntities;

public sealed class NegatableMapEntityActivation
{
    public required Branch<MapEntityLeaf> MapEntity { get; set; }
    public required bool IsActivationValueNegated { get; set; }
    internal int EffectiveValue => MapEntity.GameId * (IsActivationValueNegated ? -1 : 1);
}