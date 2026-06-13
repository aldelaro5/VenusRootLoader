using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class QuestBoardNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal QuestBoardNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.QuestBoard;

    public Branch<NpcMapEntityLeaf> BoardCaretaker
    {
        get;
        set
        {
            if (value.Leaf.Map != Map)
            {
                ThrowHelper.ThrowInvalidOperationException(
                    $"The caretaker map entity must be on the {Map.NamedId} map");
            }

            InternalData[0].Value = value.GameId;
            field = value;
        }
    }

    public Branch<DialogueLeaf> BoardCaretakerDialogueWhenQuestIsSelected
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalData[1].Value = value.GameId;
            field = value;
        }
    }

    public Branch<FlagLeaf>? FlagInteractWithCaretakerIfFlagIsFalse
    {
        get;
        set
        {
            InternalData[2].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 CameraPositionBeforeShowingQuests
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    public Vector3 CameraAnglesBeforeShowingQuests
    {
        get => InternalVectorData[1].Value;
        set => InternalVectorData[1].Value = value;
    }

    public float CameraSpeedBeforeShowingQuests
    {
        get => InternalVectorData[2].Value.x;
        set => InternalVectorData[2].Value.x = value;
    }

    public float CameraMovementTimeInSecondsBeforeShowingQuests
    {
        get => InternalVectorData[2].Value.y;
        set => InternalVectorData[2].Value.y = value;
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        Branch<NpcMapEntityLeaf> boardCaretaker,
        Branch<DialogueLeaf> boardCaretakerDialogueWhenQuestIsSelected)
    {
        base.InitializeFromNew(startingPosition, animId);
        InternalData.AddRange(Enumerable.Repeat(new Ref<int>(-1), 3 - InternalData.Count));
        InternalVectorData.AddRange(Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 3 - InternalVectorData.Count));
        BoardCaretaker = boardCaretaker;
        BoardCaretakerDialogueWhenQuestIsSelected = boardCaretakerDialogueWhenQuestIsSelected;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalData[2].Value >= 0)
            FlagInteractWithCaretakerIfFlagIsFalse = flagsRegistry.LeavesByGameIds[InternalData[2].Value];

        BoardCaretaker =
            (Branch<NpcMapEntityLeaf>)Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[0].Value]!;
        BoardCaretakerDialogueWhenQuestIsSelected = InternalData[1].Value < 0
            ? commonDialoguesRegistry.LeavesByGameIds[InternalData[1].Value]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalData[1].Value];
    }
}