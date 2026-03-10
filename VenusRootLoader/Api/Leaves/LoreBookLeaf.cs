namespace VenusRootLoader.Api.Leaves;

// TODO: Solve the LibraryShelf issue
// TODO: Patch the map dialogue with the hardcoded amount of lore books so it reflects the registry
// TODO: Patch the list type so LoreText isn't fetched once per refreshed elements of the ItemList
public sealed class LoreBookLeaf : Leaf
{
    public sealed class LoreBookLanguageData
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string FortuneTellerHint { get; set; } = "";
    }

    public LocalizedData<LoreBookLanguageData> LocalizedData { get; } = new();
    public Branch<FlagLeaf> LoreBookObtainedFlag { get; set; }
}