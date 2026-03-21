namespace VenusRootLoader.Api.Leaves;

public sealed class MedalFortuneTellerHintLeaf : Leaf
{
    internal MedalFortuneTellerHintLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public Branch<FlagLeaf> MedalObtainedFlag { get; set; }
    public LocalizedData<string> LocalizedHintText { get; } = new();
}