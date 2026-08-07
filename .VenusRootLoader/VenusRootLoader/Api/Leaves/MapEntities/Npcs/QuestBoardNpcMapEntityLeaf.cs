using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class QuestBoardNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal QuestBoardNpcMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.QuestBoard;

    public Branch<NpcMapEntityLeaf> BoardCaretakerNpc
    {
        get;
        set
        {
            if (value.Resolve().Map != Map)
            {
                ThrowHelper.ThrowInvalidOperationException(
                    $"The caretaker map entity must be on the {Map.NamedId} map");
            }

            InternalData[0].Value = value.Resolve().GameId;
            field = value;
        }
    }

    public Branch<DialogueLeaf> BoardCaretakerDialogueWhenQuestIsSelected
    {
        get;
        set
        {
            if (value.Resolve().AssociatedMap is not null && value.Resolve().AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalData[1].Value = value.Resolve().GameId;
            field = value;
        }
    }

    public Branch<FlagLeaf>? FlagInteractWithCaretakerInsteadOfShowingQuestBoardWhenFalse
    {
        get;
        set
        {
            InternalData[2].Value = value?.Resolve().GameId ?? -1;
            field = value;
        }
    }

    public Vector3 CameraPositionOffsetFromTargetBeforeShowingQuests
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

    public NpcHornInteraction HornInteraction
    {
        get
        {
            if (Modifiers.HasFlag(MapEntityModifiers.ITHD))
                return NpcHornInteraction.InteractWithHornDashOnly;
            return Modifiers.HasFlag(MapEntityModifiers.ITAH)
                ? NpcHornInteraction.InteractWithAnyHornAttack
                : NpcHornInteraction.None;
        }
        set
        {
            switch (value)
            {
                case NpcHornInteraction.None:
                    Modifiers &= ~MapEntityModifiers.ITAH;
                    Modifiers &= ~MapEntityModifiers.ITHD;
                    break;
                case NpcHornInteraction.InteractWithHornDashOnly:
                    Modifiers &= ~MapEntityModifiers.ITAH;
                    Modifiers |= MapEntityModifiers.ITHD;
                    break;
                case NpcHornInteraction.InteractWithAnyHornAttack:
                    Modifiers |= MapEntityModifiers.ITAH;
                    Modifiers &= ~MapEntityModifiers.ITHD;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(PhysicsBehavior));
                    break;
            }
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        Branch<NpcMapEntityLeaf> boardCaretakerNpc,
        Branch<DialogueLeaf> boardCaretakerDialogueWhenQuestIsSelected)
    {
        base.InitializeFromNew(startingPosition, animId);
        for (int i = 0; i < 3; i++)
            InternalData.Add(new Ref<int>(-1));
        for (int i = 0; i < 3; i++)
            InternalVectorData.Add(new Ref<Vector3>(Vector3.zero));
        BoardCaretakerNpc = boardCaretakerNpc;
        BoardCaretakerDialogueWhenQuestIsSelected = boardCaretakerDialogueWhenQuestIsSelected;
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        ILeavesRegistry<FlagLeaf> flagsRegistry = RegistryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = RegistryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalData[2].Value >= 0)
            FlagInteractWithCaretakerInsteadOfShowingQuestBoardWhenFalse =
                flagsRegistry.GetByGameId(InternalData[2].Value);

        BoardCaretakerNpc = new((NpcMapEntityLeaf)Map.Resolve().EntitiesRegistry.GetByGameId(InternalData[0].Value));
        BoardCaretakerDialogueWhenQuestIsSelected = InternalData[1].Value < 0
            ? commonDialoguesRegistry.GetByGameId(InternalData[1].Value)
            : Map.Resolve().DialoguesRegistry.GetByGameId(InternalData[1].Value);
    }
}