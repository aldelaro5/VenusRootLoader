namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class VenusHealingNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal VenusHealingNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.VenusHeal;
}