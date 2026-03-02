namespace VenusRootLoader.Api.Leaves;

public sealed class MedalFortuneTellerHintLeaf : Leaf
{
    public Branch<FlagLeaf> MedalObtainedFlag { get; set; }
    public LocalizedData<string> LocalizedHintText { get; } = new();
}