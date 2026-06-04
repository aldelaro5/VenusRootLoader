using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class QuestBoardNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal QuestBoardNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.QuestBoard;

    public Branch<MapEntityLeaf> BoardCaretakerMapEntity
    {
        get;
        set
        {
            if (value.Leaf.Map != Map)
            {
                ThrowHelper.ThrowInvalidOperationException(
                    $"The caretaker map entity must be on the {Map.NamedId} map");
            }

            InternalData[0] = value.GameId;
            field = value;
        }
    }

    public Branch<DialogueLeaf> CaretakerDialogueWhenQuestIsTaken
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalData[1] = value.GameId;
            field = value;
        }
    }

    public Branch<FlagLeaf>? FlagInteractWithCaretakerWhenFalse
    {
        get;
        set
        {
            InternalData[2] = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 CameraPositionBeforeShowingQuests
    {
        get => InternalVectorData[0];
        set => InternalVectorData[0] = value;
    }

    public Vector3 CameraAnglesBeforeShowingQuests
    {
        get => InternalVectorData[1];
        set => InternalVectorData[1] = value;
    }

    public float CameraSpeedBeforeShowingQuests
    {
        get => InternalVectorData[2].x;
        set => InternalVectorData[2] = new Vector3(
            value,
            InternalVectorData[2].y,
            InternalVectorData[2].z);
    }

    public float CameraMovementTimeInSecondsBeforeShowingQuests
    {
        get => InternalVectorData[2].y;
        set => InternalVectorData[2] = new Vector3(
            InternalVectorData[2].x,
            value,
            InternalVectorData[2].z);
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange(Enumerable.Repeat(-1, 3 - InternalData.Count));
        InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 3 - InternalVectorData.Count));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalData[2] >= 0)
            FlagInteractWithCaretakerWhenFalse = flagsRegistry.LeavesByGameIds[InternalData[2]];

        BoardCaretakerMapEntity = Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[0]];
        CaretakerDialogueWhenQuestIsTaken = InternalData[1] < 0
            ? commonDialoguesRegistry.LeavesByGameIds[InternalData[1]]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalData[1]];
    }
}