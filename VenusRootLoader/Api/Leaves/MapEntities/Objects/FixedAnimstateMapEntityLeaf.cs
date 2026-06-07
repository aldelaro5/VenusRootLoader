using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class FixedAnimstateMapEntityLeaf : MapEntityLeaf
{
    internal FixedAnimstateMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.FixedAnim;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public bool HasNoCcolAndRigidGravity
    {
        get => InternalData[0].Value == 1;
        set => InternalData[0].Value = value ? 1 : 0;
    }

    public int Animstate { get => InternalData[1].Value; set => InternalData[1].Value = value; }

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

    public float EntityBobSpeed { get => InternalBobSpeed; set => InternalBobSpeed = value; }
    public float EntityBobRange { get => InternalBobRange; set => InternalBobRange = value; }
    public float EntityInitialHeight { get => InternalInitialHeight; set => InternalInitialHeight = value; }
    public float EntityCapsuleColliderRadius { get => InternalCcolRadius; set => InternalCcolRadius = value; }
    public float EntityCapsulerColliderHeight { get => InternalCcolHeight; set => InternalCcolHeight = value; }
    public bool EntitySpriteIsFlipped { get => InternalIsFlipped; set => InternalIsFlipped = value; }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(0), new(0)]);
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