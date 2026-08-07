using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DialogueTriggers;

public sealed class DialogueTriggerZoneMapEntityLeaf : DialogueTriggerMapEntityLeaf
{
    internal DialogueTriggerZoneMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public Branch<DialogueLeaf> DialogueToProcessWhenTriggered
    {
        get;
        set
        {
            if (value.Resolve().AssociatedMap is not null && value.Resolve().AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalData[0].Value = value.Resolve().GameId;
            field = value;
        }
    }

    public bool IsOneShotTrigger { get => InternalData[1].Value != 1; set => InternalData[1].Value = value ? 0 : 1; }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public Branch<FlagLeaf>? FlagSetToTrueWhenTriggered
    {
        get;
        set
        {
            InternalActivationFlagId = value?.Resolve().GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<DialogueLeaf> dialogueToProcessWhenTriggered,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter)
    {
        InternalData.AddRange([new(-1), new(0), new(0)]);
        DialogueToProcessWhenTriggered = dialogueToProcessWhenTriggered;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        TriggerBoxColliderSize = triggerBoxColliderSize;
        TriggerBoxColliderCenter = triggerBoxColliderCenter;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting()
    {
        if (InternalData.Count < 3)
        {
            int count = InternalData.Count;
            for (int i = 0; i < 3 - count; i++)
                InternalData.Add(new Ref<int>(0));
        }

        ILeavesRegistry<FlagLeaf> flagsRegistry = RegistryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = RegistryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalActivationFlagId > 0)
            FlagSetToTrueWhenTriggered = new(flagsRegistry.GetByGameId(InternalActivationFlagId));

        DialogueToProcessWhenTriggered = InternalData[0].Value < 0
            ? commonDialoguesRegistry.GetByGameId(InternalData[0].Value)
            : Map.Resolve().DialoguesRegistry.GetByGameId(InternalData[0].Value);
    }
}