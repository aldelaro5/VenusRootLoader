namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndGates;

public abstract class AndGateMapEntityLeaf : ObjectMapEntityLeaf
{
    protected AndGateMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDGate;

    protected void InitializeFromNew()
    {
        InternalStartingPosition = new(0f, 9999f, 0f);
    }
}