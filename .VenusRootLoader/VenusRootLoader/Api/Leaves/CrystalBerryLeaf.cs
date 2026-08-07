using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class CrystalBerryLeaf : Leaf
{
    internal CrystalBerryLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }

    public LocalizedData<string> LocalizedFortuneTellerHint { get; } = new();
}