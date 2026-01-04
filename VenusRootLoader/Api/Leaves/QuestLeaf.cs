namespace VenusRootLoader.Api.Leaves;

internal sealed class QuestLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal Dictionary<int, string> Name { get; set; } = new();
    internal Dictionary<int, string> Description { get; set; } = new();
    internal Dictionary<int, string> Sender { get; set; } = new();

    internal int BoundTakenFlagId { get; set; }
    internal int EnemyPortraitsSpriteIndexForIcon { get; set; }
    internal int Difficulty { get; set; }

    internal List<int> RequiredFlagIds { get; } = new();
}