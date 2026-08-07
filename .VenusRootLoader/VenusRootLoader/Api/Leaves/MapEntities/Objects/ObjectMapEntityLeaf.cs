namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public abstract class ObjectMapEntityLeaf : MapEntityLeaf
{
    protected ObjectMapEntityLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;
}