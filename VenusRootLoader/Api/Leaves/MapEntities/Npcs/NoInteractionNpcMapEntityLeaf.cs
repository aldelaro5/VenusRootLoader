namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class NoInteractionNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal NoInteractionNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;
}