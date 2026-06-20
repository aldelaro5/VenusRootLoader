using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class CameraChangeZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal CameraChangeZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.CameraChange;

    public Vector3? CameraPositionOffsetFromTargetWhenTriggered
    {
        get => InternalData[0].Value != 1 ? null : InternalVectorData[0].Value;
        set
        {
            if (value is null)
            {
                InternalData[0].Value = 0;
                InternalVectorData[0].Value = Vector3.zero;
                return;
            }

            InternalData[0].Value = 1;
            InternalVectorData[0].Value = value.Value;
        }
    }

    public (Vector3 lowerBounds, Vector3 upperBounds)? CameraBoundsWhenTriggered
    {
        get => InternalData[1].Value != 1 ||
               (InternalVectorData[2].Value.magnitude <= 0.1f && InternalVectorData[1].Value.magnitude <= 0.1f)
            ? null
            : (InternalVectorData[2].Value, InternalVectorData[1].Value);
        set
        {
            if (value is null ||
                (value.Value.lowerBounds.magnitude <= 0.1f && value.Value.upperBounds.magnitude <= 0.1f))
            {
                InternalData[1].Value = 0;
                InternalVectorData[2].Value = Vector3.zero;
                InternalVectorData[1].Value = Vector3.zero;
                return;
            }

            InternalData[1].Value = 1;
            InternalVectorData[2].Value = value.Value.lowerBounds;
            InternalVectorData[1].Value = value.Value.upperBounds;
        }
    }

    public float? CameraMovementSpeedWhenTriggered
    {
        get => InternalData[2].Value != 1 ? null : InternalVectorData[3].Value.x;
        set
        {
            if (value is null)
            {
                InternalData[2].Value = 0;
                InternalVectorData[3].Value.x = 0f;
                return;
            }

            InternalData[2].Value = 1;
            InternalVectorData[3].Value.x = value.Value;
        }
    }

    public Vector3? CameraAnglesOffsetFromTargetWhenTriggered
    {
        get => InternalData[3].Value != 1 ? null : InternalVectorData[4].Value;
        set
        {
            if (value is null)
            {
                InternalData[3].Value = 0;
                InternalVectorData[4].Value = Vector3.zero;
                return;
            }

            InternalData[3].Value = 1;
            InternalVectorData[4].Value = value.Value;
        }
    }

    public int? CameraTargetEntityIdWhenTriggered
    {
        get => InternalData[4].Value != 1 ? null : InternalData[5].Value;
        set
        {
            if (value is null)
            {
                InternalData[4].Value = 0;
                InternalData[5].Value = 0;
                return;
            }

            InternalData[4].Value = 1;
            InternalData[5].Value = value.Value;
        }
    }

    public Vector3? CameraTargetPositionWhenTriggered
    {
        get => InternalData[6].Value != 1 ? null : InternalVectorData[5].Value;
        set
        {
            if (value is null)
            {
                InternalData[6].Value = 0;
                InternalVectorData[5].Value = Vector3.zero;
                return;
            }

            InternalData[6].Value = 1;
            InternalVectorData[5].Value = value.Value;
        }
    }

    public float? CameraRotationSpeedWhenTriggered
    {
        get => InternalData[7].Value != 1 ? null : InternalVectorData[3].Value.y;
        set
        {
            if (value is null)
            {
                InternalData[7].Value = 0;
                InternalVectorData[3].Value.y = 0f;
                return;
            }

            InternalData[7].Value = 1;
            InternalVectorData[3].Value.y = value.Value;
        }
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter)
    {
        for (int i = 0; i < 8; i++)
            InternalData.Add(new Ref<int>(0));
        for (int i = 0; i < 6; i++)
            InternalVectorData.Add(new Ref<Vector3>(Vector3.zero));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        TriggerBoxColliderSize = triggerBoxColliderSize;
        TriggerBoxColliderCenter = triggerBoxColliderCenter;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 8)
        {
            for (int i = 0; i < 8 - InternalData.Count; i++)
                InternalData.Add(new Ref<int>(0));
        }

        if (InternalVectorData.Count < 6)
        {
            for (int i = 0; i < 6 - InternalVectorData.Count; i++)
                InternalVectorData.Add(new Ref<Vector3>(Vector3.zero));
        }
    }
}