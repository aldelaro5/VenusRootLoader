using System.Collections;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class BattleEventDialogueLeaf : Leaf
{
    public delegate IEnumerator BattleEventDialogueHandler(BattleControl battle);

    internal BattleEventDialogueLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public BattleEventDialogueHandler Handler { get; set; } = null!;

    [LeafInitializeFromNew]
    internal void InitializeFromNew(BattleEventDialogueHandler handler)
    {
        Handler = handler;
    }
}