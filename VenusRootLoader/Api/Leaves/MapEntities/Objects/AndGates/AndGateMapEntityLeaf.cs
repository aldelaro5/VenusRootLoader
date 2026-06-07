namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndGates;

public abstract class AndGateMapEntityLeaf : MapEntityLeaf
{
    protected AndGateMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDGate;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = -1;
        InternalStartingPosition = new(0f, 9999f, 0f);
    }
}