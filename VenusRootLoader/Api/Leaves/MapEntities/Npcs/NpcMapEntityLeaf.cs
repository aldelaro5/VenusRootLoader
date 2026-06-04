using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public abstract class NpcMapEntityLeaf : MapEntityLeaf
{
    protected NpcMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
        Behaviors = new(this);
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.NPC;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;

    public MapEntityBehaviors Behaviors { get; }

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public float EntityBobSpeed { get => InternalBobSpeed; set => InternalBobSpeed = value; }
    public float EntityBobRange { get => InternalBobRange; set => InternalBobRange = value; }
    public float EntityInitialHeight { get => InternalInitialHeight; set => InternalInitialHeight = value; }
    public float EntityCapsuleColliderRadius { get => InternalCcolRadius; set => InternalCcolRadius = value; }
    public float EntityCapsulerColliderHeight { get => InternalCcolHeight; set => InternalCcolHeight = value; }
    public bool EntitySpriteIsFlipped { get => InternalIsFlipped; set => InternalIsFlipped = value; }
    public Vector3 EntityEmoticonOffset { get => InternalEmoticonOffset; set => InternalEmoticonOffset = value; }
    public float EntitySpeed { get => InternalSpeed; set => InternalSpeed = value; }
    public float MovementRadius { get => InternalRadiusLimit; set => InternalRadiusLimit = value; }
    public float BehaviorAndInteractRangeRadius { get => InternalRadius; set => InternalRadius = value; }

    public required NpcEmoticon FallbackEmoticon
    {
        get => (NpcEmoticon)(int)InternalEmoticonFlags[0].y;
        set => InternalEmoticonFlags[0] = new(InternalEmoticonFlags[0].x, (int)value);
    }

    public ReadOnlyCollection<NpcConditionalEmoticon> ConditionalEmoticons { get; private set; } =
        new List<NpcConditionalEmoticon>().AsReadOnly();

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = 0;
        InternalEmoticonFlags.Add(new(-1, 0));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();

        Behaviors.InitializeBehaviorFromExisting(registryResolver);

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        List<NpcConditionalEmoticon> emoticons = InternalEmoticonFlags
            .Skip(1)
            .Select(emoticon => new NpcConditionalEmoticon
            {
                RequiredFlag = (int)emoticon.x >= 0
                    ? new(flagsRegistry.LeavesByGameIds[(int)emoticon.x])
                    : null,
                Emoticon = (NpcEmoticon)(int)emoticon.y,
            })
            .ToList();
        ChangeEmoticons(emoticons);
    }

    public void ChangeEmoticons(List<NpcConditionalEmoticon> emoticons)
    {
        if (InternalEmoticonFlags.Count > 1)
            InternalEmoticonFlags.RemoveRange(1, InternalEmoticonFlags.Count - 1);

        foreach (NpcConditionalEmoticon emoticon in emoticons)
            InternalEmoticonFlags.Add(new(emoticon.RequiredFlag?.GameId ?? -1, (int)emoticon.Emoticon));

        ConditionalEmoticons = emoticons.AsReadOnly();
    }
}