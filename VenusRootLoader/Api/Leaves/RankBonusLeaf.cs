namespace VenusRootLoader.Api.Leaves;

internal sealed class RankBonusLeaf : Leaf
{
    internal int Rank { get; set; }
    internal int BonusType { get; set; }
    internal int FirstParameterValue { get; set; }
    internal int SecondParameterValue { get; set; }
    internal int ThirdParameterValue { get; set; }
}