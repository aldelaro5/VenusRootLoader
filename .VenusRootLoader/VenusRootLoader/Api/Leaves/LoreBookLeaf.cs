using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

// TODO: Solve the LibraryShelf issue
[ExposeFromVenus]
public sealed class LoreBookLeaf : Leaf
{
    public sealed class LoreBookLanguageData
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string FortuneTellerHint { get; set; } = "";
    }

    internal LoreBookLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public LocalizedData<LoreBookLanguageData> LocalizedData { get; } = new();
    public Branch<FlagLeaf> LoreBookObtainedFlag { get; set; } = null!;

    [LeafInitializeFromNew]
    internal void InitializeFromNew(Branch<FlagLeaf> loreBookObtainedFlag)
    {
        LoreBookObtainedFlag = loreBookObtainedFlag;
    }
}