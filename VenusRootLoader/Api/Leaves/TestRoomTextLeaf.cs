namespace VenusRootLoader.Api.Leaves;

public sealed class TestRoomTextLeaf : Leaf
{
    internal TestRoomTextLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public string Text { get; set; } = "";
}