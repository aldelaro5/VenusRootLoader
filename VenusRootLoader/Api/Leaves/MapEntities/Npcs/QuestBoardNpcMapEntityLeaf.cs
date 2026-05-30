using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class QuestBoardNpcMapEntityLeaf : MapEntityLeaf
{
    internal QuestBoardNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.NPC;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.QuestBoard;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public MapEntityLeaf BoardCaretakerMapEntityLeaf
    {
        get => Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[0]];
        set
        {
            if (value.Map != Map)
            {
                ThrowHelper.ThrowInvalidOperationException(
                    "The caretaker map entity must be on the same map as this NPC");
            }

            InternalData[0] = value.GameId;
        }
    }

    public int CaretakerDialogueIdWhenQuestIsTaken
    {
        get => InternalData[1];
        set => InternalData[1] = value;
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

    public float InteractRangeRadius
    {
        get => InternalRadius;
        set => InternalRadius = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange(Enumerable.Repeat(-1, 3 - InternalData.Count));
        InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 3 - InternalVectorData.Count));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        FlagInteractWithCaretakerWhenFalse = InternalData[2] >= 0
            ? new(flagsRegistry.LeavesByGameIds[InternalData[2]])
            : null;
    }
}