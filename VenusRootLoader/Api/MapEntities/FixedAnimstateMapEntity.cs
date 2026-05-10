using UnityEngine;
using VenusRootLoader.Api.Common;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class FixedAnimstateMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.FixedAnim;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public bool HasNoCcolAndRigidGravity { get => InternalData[0] == 1; set => InternalData[0] = value ? 1 : 0; }
    public int Animstate { get => InternalData[1]; set => InternalData[1] = value; }

    public BoxColliderInfo? BoxCollider
    {
        get;
        set
        {
            InternalHaxBoxCol = value is not null;
            InternalBoxColIsTrigger = value?.IsTrigger ?? false;
            InternalBoxColSize = value?.Size ?? Vector3.zero;
            InternalBoxColCenter = value?.Center ?? Vector3.zero;
            field = value;
        }
    }

    internal FixedAnimstateMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 0]);
        InternalAnimIdOrItemId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
        if (InternalHaxBoxCol)
        {
            BoxCollider = new()
            {
                IsTrigger = InternalBoxColIsTrigger,
                Size = InternalBoxColSize,
                Center = InternalBoxColCenter
            };
        }
    }
}