using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class FixedAnimstateMapEntityLeaf : ObjectMapEntityLeaf
{
    internal FixedAnimstateMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.FixedAnim;

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public bool HasNoCapsuleColliderAndNoGravity
    {
        get => InternalData[0].Value == 1;
        set => InternalData[0].Value = value ? 1 : 0;
    }

    public int Animstate { get => InternalData[1].Value; set => InternalData[1].Value = value; }

    public bool BoxColliderIsPresent { get => InternalHaxBoxCol; set => InternalHaxBoxCol = value; }
    public bool BoxColliderIsTrigger { get => InternalBoxColIsTrigger; set => InternalBoxColIsTrigger = value; }
    public Vector3 BoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 BoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public float EntityBobSpeedWhileAirborne { get => InternalBobSpeed; set => InternalBobSpeed = value; }
    public float EntityBobRangeWhileAirborne { get => InternalBobRange; set => InternalBobRange = value; }
    public float EntityInitialAltitudeFromGround { get => InternalInitialHeight; set => InternalInitialHeight = value; }
    public float EntityCapsuleColliderRadius { get => InternalCcolRadius; set => InternalCcolRadius = value; }
    public float EntityCapsulerColliderHeight { get => InternalCcolHeight; set => InternalCcolHeight = value; }
    public bool EntitySpriteStartsFlipped { get => InternalIsFlipped; set => InternalIsFlipped = value; }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(0), new(0)]);
        InternalAnimIdOrItemId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}