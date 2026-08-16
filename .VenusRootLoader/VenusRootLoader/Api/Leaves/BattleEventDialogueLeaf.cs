using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

// TODO: Properly implement this leaf
[ExposeFromVenus]
public sealed class BattleEventDialogueLeaf : Leaf
{
    internal BattleEventDialogueLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }
}