using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class ItemsStorageNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal ItemsStorageNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.StorageAnt;

    public Branch<DialogueLeaf>? SpyDialogue
    {
        get;
        set
        {
            InternalSpyDialogueId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalSpyDialogueId != -1)
        {
            SpyDialogue = InternalSpyDialogueId < 0
                ? commonDialoguesRegistry.LeavesByGameIds[InternalSpyDialogueId]
                : Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalSpyDialogueId];
        }
    }
}