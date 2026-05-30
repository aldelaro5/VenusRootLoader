namespace VenusRootLoader.Api.Leaves.MapEntities;

public sealed class NegatableFlag
{
    public required Branch<FlagLeaf> Flag { get; set; }
    public required bool IsValueNegated { get; set; }
    internal int EffectiveValue => Flag.GameId * (IsValueNegated ? -1 : 1);
}