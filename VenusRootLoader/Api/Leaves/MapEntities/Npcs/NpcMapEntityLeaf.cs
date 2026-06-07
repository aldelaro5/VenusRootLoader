using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public abstract class NpcMapEntityLeaf : MapEntityLeaf
{
    protected NpcMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
        _conditionalEmoticons = new(InternalEmoticonFlags, 1, x => x.Vector2Ref);
        BehaviorSystem = new(this);
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.NPC;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;

    public MapEntityBehaviorSystem BehaviorSystem { get; }

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public float EntityBobSpeedWhileAirborne { get => InternalBobSpeed; set => InternalBobSpeed = value; }
    public float EntityBobRangeWhileAirborne { get => InternalBobRange; set => InternalBobRange = value; }
    public float EntityInitialAltitudeFromGround { get => InternalInitialHeight; set => InternalInitialHeight = value; }
    public float EntityCapsuleColliderRadius { get => InternalCcolRadius; set => InternalCcolRadius = value; }
    public float EntityCapsulerColliderHeight { get => InternalCcolHeight; set => InternalCcolHeight = value; }
    public bool EntitySpriteStartsFlipped { get => InternalIsFlipped; set => InternalIsFlipped = value; }
    public Vector3 EntityEmoticonOffset { get => InternalEmoticonOffset; set => InternalEmoticonOffset = value; }
    public float EntityMovementSpeed { get => InternalSpeed; set => InternalSpeed = value; }
    public float MovementRadius { get => InternalRadiusLimit; set => InternalRadiusLimit = value; }
    public float BehaviorSystemAndInteractRangeRadius { get => InternalRadius; set => InternalRadius = value; }

    public NpcEmoticon FallbackEmoticon
    {
        get => (NpcEmoticon)(int)InternalEmoticonFlags[0].Value.y;
        set => InternalEmoticonFlags[0].Value.y = (int)value;
    }

    private readonly ListRefWrapper<NpcConditionalEmoticon, Vector2> _conditionalEmoticons;
    public IList<NpcConditionalEmoticon> ConditionalEmoticons => _conditionalEmoticons;

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = 0;
        InternalEmoticonFlags.Add(new(new(-1, 0)));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();

        BehaviorSystem.InitializeBehaviorFromExisting(registryResolver);

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        _conditionalEmoticons.SynchronizeFromExistingData(
            InternalEmoticonFlags
            .Skip(1)
            .Select(emoticon => new NpcConditionalEmoticon
            {
                RequiredFlag = (int)emoticon.Value.x >= 0
                    ? new(flagsRegistry.LeavesByGameIds[(int)emoticon.Value.x])
                    : null,
                Emoticon = (NpcEmoticon)(int)emoticon.Value.y,
            })
            .ToList());
    }
}