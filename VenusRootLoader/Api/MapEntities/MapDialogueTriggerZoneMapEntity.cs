using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class MapDialogueTriggerZoneMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DialogueTrigger;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public int MapDialogueLineIdToProcessWhenTriggered
    {
        get => InternalData[0];
        set => InternalData[0] = value;
    }

    public bool IsOneShotTrigger { get => InternalData[1] != 1; set => InternalData[1] = value ? 0 : 1; }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    internal MapDialogueTriggerZoneMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([-1, 0, 0]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = Vector3.one;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(0, 3 - InternalData.Count));
    }
}