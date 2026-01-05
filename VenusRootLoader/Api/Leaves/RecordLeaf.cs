namespace VenusRootLoader.Api.Leaves;

internal sealed class RecordLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal Dictionary<int, string> Name { get; } = new();
    internal Dictionary<int, string> Description { get; } = new();

    internal int EnemyPortraitsSpriteIndex { get; set; }
}