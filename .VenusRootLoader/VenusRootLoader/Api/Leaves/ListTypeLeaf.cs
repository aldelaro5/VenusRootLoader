using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

// TODO: Fix the inlist issue.
[ExposeFromVenus]
public sealed class ListTypeLeaf : Leaf
{
    public delegate void ListTypeSetupHandler(MainManager.ShowItemListSetupContext context);

    public delegate void ListTypeRenderer(MainManager.ShowItemListRenderContext context);

    internal ListTypeLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public ListTypeSetupHandler SetupHandler { get; set; } = null!;
    public ListTypeRenderer? Renderer { get; set; }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(ListTypeSetupHandler setupHandler, ListTypeRenderer? renderer)
    {
        SetupHandler = setupHandler;
        Renderer = renderer;
    }
}