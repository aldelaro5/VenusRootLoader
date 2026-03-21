using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Api.Common;

public sealed class LimitFlag
{
    public required Branch<FlagLeaf> Flag { get; set; }
    public bool FailsWholeConditionWhenFlagIsTrue { get; set; }
}