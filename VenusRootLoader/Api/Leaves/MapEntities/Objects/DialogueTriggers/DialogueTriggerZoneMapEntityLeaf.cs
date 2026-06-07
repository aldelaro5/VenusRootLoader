using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DialogueTriggers;

public sealed class DialogueTriggerZoneMapEntityLeaf : MapEntityLeaf
{
    internal DialogueTriggerZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DialogueTrigger;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

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

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(-1), new(0), new(0)]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = Vector3.one;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 3 - InternalData.Count));

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);

        DialogueToProcessWhenTriggered = InternalData[0].Value < 0
            ? commonDialoguesRegistry.LeavesByGameIds[InternalData[0].Value]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalData[0].Value];
    }
}