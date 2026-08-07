using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class MedalFortuneTellerHintLeaf : Leaf
{
    internal MedalFortuneTellerHintLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public Branch<FlagLeaf> MedalObtainedFlag { get; set; }
    public LocalizedData<string> LocalizedHintText { get; } = new();
}