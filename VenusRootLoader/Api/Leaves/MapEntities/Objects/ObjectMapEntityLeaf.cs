namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public abstract class ObjectMapEntityLeaf : MapEntityLeaf
{
    protected ObjectMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;
}