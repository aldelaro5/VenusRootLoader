using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class SpinningCrankMapEntityLeaf : MapEntityLeaf
{
    internal SpinningCrankMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ScrewSwitch;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public float RateIncreaseWhenSpinning
    {
        get => InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value;
    }

    public float RateDecreaseWhenNotSpinning
    {
        get => InternalVectorData[0].Value.y;
        set => InternalVectorData[0].Value.y = value;
    }

    public float TotalSpinCapacity
    {
        get => InternalVectorData[0].Value.z;
        set => InternalVectorData[0].Value.z = value;
    }

    public Vector3 SpinningRotationAngles
    {
        get => InternalVectorData[1].Value;
        set => InternalVectorData[1].Value = value;
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    internal override void InitializeFromNew()
    {
        InternalVectorData.AddRange([new(new(1f, 0.5f, 300f)), new(new(0f, -20f, 0f))]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.ScrewSwitch - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1f, 2f, 1f);
        InternalBoxColCenter = new(0f, 1f, 0f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}