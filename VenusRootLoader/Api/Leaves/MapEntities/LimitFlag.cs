namespace VenusRootLoader.Api.Leaves.MapEntities;

public sealed class LimitFlag
{
    public required Branch<FlagLeaf> Flag { get; set; }
    public bool FailsWholeConditionWhenFlagIsTrue { get; set; }
}