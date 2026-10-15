using System.Collections;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class SetTextCommandLeaf : Leaf
{
    public delegate IEnumerator SetTextCommandHandler(MainManager.SetTextCommandContext context);

    internal SetTextCommandLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }

    public SetTextCommandHandler? Handler { get; set; }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(SetTextCommandHandler commandHandler)
    {
        Handler = commandHandler;
    }
}