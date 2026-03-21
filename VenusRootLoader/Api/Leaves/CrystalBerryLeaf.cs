namespace VenusRootLoader.Api.Leaves;

public sealed class CrystalBerryLeaf : Leaf
{
    internal CrystalBerryLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId) { }

    public LocalizedData<string> LocalizedFortuneTellerHint { get; } = new();
}