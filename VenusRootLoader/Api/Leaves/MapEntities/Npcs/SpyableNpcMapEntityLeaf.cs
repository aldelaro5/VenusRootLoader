using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public abstract class SpyableNpcMapEntityLeaf : NpcMapEntityLeaf
{
    protected SpyableNpcMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public Branch<DialogueLeaf>? SpyDialogue
    {
        get;
        set
        {
            InternalSpyDialogueId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        Branch<DialogueLeaf>? spyDialogue)
    {
        base.InitializeFromNew(startingPosition, animId);
        SpyDialogue = spyDialogue;
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