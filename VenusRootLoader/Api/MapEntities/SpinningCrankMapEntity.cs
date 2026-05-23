using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class SpinningCrankMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ScrewSwitch;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public float RateIncreaseWhenSpinning
    {
        get => InternalVectorData[0].x;
        set => InternalVectorData[0] = new(value, InternalVectorData[0].y, InternalVectorData[0].z);
    }

    public float RateDecreaseWhenNotSpinning
    {
        get => InternalVectorData[0].y;
        set => InternalVectorData[0] = new(InternalVectorData[0].x, value, InternalVectorData[0].z);
    }

    public float TotalSpinCapacity
    {
        get => InternalVectorData[0].z;
        set => InternalVectorData[0] = new(InternalVectorData[0].x, InternalVectorData[0].y, value);
    }

    public Vector3 SpinningRotationAngles { get => InternalVectorData[1]; set => InternalVectorData[1] = value; }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    internal SpinningCrankMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalVectorData.AddRange([new(1f, 0.5f, 300f), new(0f, -20f, 0f)]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.ScrewSwitch - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1f, 2f, 1f);
        InternalBoxColCenter = new(0f, 1f, 0f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}