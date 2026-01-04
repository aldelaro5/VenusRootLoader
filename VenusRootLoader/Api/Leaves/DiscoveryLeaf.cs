namespace VenusRootLoader.Api.Leaves;

internal sealed class DiscoveryLeaf : ILeaf
{
    internal sealed class DiscoveryDescriptionPage
    {
        internal string Text { get; set; } = "<NO CONTENT>";
        internal int? RequiredFlagGameId { get; set; }
    }

    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal int EnemyPortraitsSpriteIndex { get; set; }

    internal Dictionary<int, string> Name { get; } = new();
    internal Dictionary<int, List<DiscoveryDescriptionPage>> PaginatedDescription { get; } = new();
}