using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DialogueTriggers;

public sealed class DialogueTriggerZoneMapEntityLeaf : DialogueTriggerMapEntityLeaf
{
    internal DialogueTriggerZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<DialogueLeaf> DialogueToProcessWhenTriggered
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalData[0].Value = value.GameId;
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
            InternalActivationFlagId = value?.GameId ?? -1;
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

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 3 - InternalData.Count));

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalActivationFlagId > 0)
            FlagSetToTrueWhenTriggered = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);

        DialogueToProcessWhenTriggered = InternalData[0].Value < 0
            ? commonDialoguesRegistry.LeavesByGameIds[InternalData[0].Value]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalData[0].Value];
    }
}