using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: height and bobbing aren't supported because this is an Object and all Objects are excluded in UpdateHeight, patch the game to add support.
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

    public float EntityCapsuleColliderRadius { get => InternalCcolRadius; set => InternalCcolRadius = value; }
    public float EntityCapsulerColliderHeight { get => InternalCcolHeight; set => InternalCcolHeight = value; }
    public bool EntitySpriteStartsFlipped { get => InternalIsFlipped; set => InternalIsFlipped = value; }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        int animstate)
    {
        InternalData.AddRange([new(0), new(0)]);
        AnimId = animId;
        Animstate = animstate;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}