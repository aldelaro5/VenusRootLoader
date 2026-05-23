using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class CameraChangeMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.CameraChange;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Vector3? CameraPositionOffsetFromTargetWhenTriggered
    {
        get => InternalData[0] != 1 ? null : InternalVectorData[0];
        set
        {
            if (value is null)
            {
                InternalData[0] = 0;
                InternalVectorData[0] = Vector3.zero;
                return;
            }

            InternalData[0] = 1;
            InternalVectorData[0] = value.Value;
        }
    }

    public (Vector3 lowerBounds, Vector3 upperBounds)? CameraBoundsWhenTriggered
    {
        get => InternalData[1] != 1 ||
               (InternalVectorData[2].magnitude <= 0.1f && InternalVectorData[1].magnitude <= 0.1f)
            ? null
            : (InternalVectorData[2], InternalVectorData[1]);
        set
        {
            if (value is null ||
                (value.Value.lowerBounds.magnitude <= 0.1f && value.Value.upperBounds.magnitude <= 0.1f))
            {
                InternalData[1] = 0;
                InternalVectorData[2] = Vector3.zero;
                InternalVectorData[1] = Vector3.zero;
                return;
            }

            InternalData[1] = 1;
            InternalVectorData[2] = value.Value.lowerBounds;
            InternalVectorData[1] = value.Value.upperBounds;
        }
    }

    public float? CameraMovementSpeedWhenTriggered
    {
        get => InternalData[2] != 1 ? null : InternalVectorData[3].x;
        set
        {
            if (value is null)
            {
                InternalData[2] = 0;
                InternalVectorData[3] = new(0f, InternalVectorData[3].y, InternalVectorData[3].z);
                return;
            }

            InternalData[2] = 1;
            InternalVectorData[3] = new(value.Value, InternalVectorData[3].y, InternalVectorData[3].z);
        }
    }

    public Vector3? CameraAnglesOffsetFromTargetWhenTriggered
    {
        get => InternalData[3] != 1 ? null : InternalVectorData[4];
        set
        {
            if (value is null)
            {
                InternalData[3] = 0;
                InternalVectorData[4] = Vector3.zero;
                return;
            }

            InternalData[3] = 1;
            InternalVectorData[4] = value.Value;
        }
    }

    public int? CameraTargetEntityIdWhenTriggered
    {
        get => InternalData[4] != 1 ? null : InternalData[5];
        set
        {
            if (value is null)
            {
                InternalData[4] = 0;
                InternalData[5] = 0;
                return;
            }

            InternalData[4] = 1;
            InternalData[5] = value.Value;
        }
    }

    public Vector3? CameraTargetPositionWhenTriggered
    {
        get => InternalData[6] != 1 ? null : InternalVectorData[5];
        set
        {
            if (value is null)
            {
                InternalData[6] = 0;
                InternalVectorData[5] = Vector3.zero;
                return;
            }

            InternalData[6] = 1;
            InternalVectorData[5] = value.Value;
        }
    }

    public float? CameraRotationSpeedWhenTriggered
    {
        get => InternalData[7] != 1 ? null : InternalVectorData[3].y;
        set
        {
            if (value is null)
            {
                InternalData[7] = 0;
                InternalVectorData[3] = new(InternalVectorData[3].x, 0f, InternalVectorData[3].z);
                return;
            }

            InternalData[7] = 1;
            InternalVectorData[3] = new(InternalVectorData[3].x, value.Value, InternalVectorData[3].z);
        }
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    internal CameraChangeMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange(Enumerable.Repeat(0, 8));
        InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 6));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = Vector3.one;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 8)
            InternalData.AddRange(Enumerable.Repeat(0, 8 - InternalData.Count));
        if (InternalVectorData.Count < 6)
            InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 6 - InternalVectorData.Count));
    }
}