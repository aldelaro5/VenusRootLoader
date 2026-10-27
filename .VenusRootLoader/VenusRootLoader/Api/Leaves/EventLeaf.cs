using System.Collections;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class EventLeaf : Leaf
{
    public delegate IEnumerator EventHandler(EventControl events);

    internal EventLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }

    public EventHandler Handler { get; set; } = null!;

    [LeafInitializeFromNew]
    internal void InitializeFromNew(EventHandler handler)
    {
        Handler = handler;
    }
}