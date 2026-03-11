namespace VenusRootLoader.Api.Leaves;

// TODO: Solve the LibraryShelf issue
// TODO: Patch the map dialogue with the hardcoded amount of lore books so it reflects the registry
// TODO: Patch the list type so LoreText isn't fetched once per refreshed elements of the ItemList
internal sealed class LoreBookLeaf : Leaf
{
    internal sealed class LoreBookLanguageData
    {
        internal string Title { get; set; } = "";
        internal string Content { get; set; } = "";
        internal string FortuneTellerHint { get; set; } = "";
    }

    internal LocalizedData<LoreBookLanguageData> LocalizedData { get; } = new();
    internal Branch<FlagLeaf> LoreBookObtainedFlag { get; set; }
}