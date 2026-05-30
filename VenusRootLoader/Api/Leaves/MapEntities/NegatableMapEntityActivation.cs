namespace VenusRootLoader.Api.Leaves.MapEntities;

public sealed class NegatableMapEntityActivation
{
    public required MapEntityLeaf MapEntityLeaf { get; set; }
    public required bool IsActivationValueNegated { get; set; }
    internal int EffectiveValue => MapEntityLeaf.GameId * (IsActivationValueNegated ? -1 : 1);
}