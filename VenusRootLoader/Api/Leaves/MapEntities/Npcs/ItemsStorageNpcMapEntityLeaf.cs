using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class ItemsStorageNpcMapEntityLeaf : SpyableNpcMapEntityLeaf
{
    internal ItemsStorageNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.StorageAnt;
}