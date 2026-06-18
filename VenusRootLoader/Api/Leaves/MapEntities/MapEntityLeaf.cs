using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities;

public abstract class MapEntityLeaf : Leaf
{
    private readonly string[] _modifiersNames = [.. Enum.GetNames(typeof(MapEntityModifiers)).Skip(1)];
    internal MapEntityModifiers Modifiers { get; set; }

    public string BaseGameObjectName
    {
        get;
        set
        {
            if (_modifiersNames.Any(value.Contains))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(BaseGameObjectName),
                    $"{value} cannot contains any of the following as they are internal modifiers: " +
                    $"{string.Join(", ", _modifiersNames)}.");
            }

            field = value;
        }
    } = $"Unnamed {nameof(MapEntityLeaf)}";

    public Branch<MapLeaf> Map { get; internal set; }

    internal abstract NPCControl.NPCType Type { get; }
    internal abstract NPCControl.ObjectTypes ObjectType { get; }

    protected internal NPCControl.Interaction OriginalInteraction { get; internal set; }
    internal abstract NPCControl.Interaction Interaction { get; }

    internal MapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal abstract void InitializeFromExisting(IRegistryResolver registryResolver);

    public bool IsHologram
    {
        get => Modifiers.HasFlag(MapEntityModifiers.Holo);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.Holo;
            else
                Modifiers &= ~MapEntityModifiers.Holo;
        }
    }

    public MapEntityPhysicsBehavior PhysicsBehavior
    {
        get
        {
            if (Modifiers.HasFlag(MapEntityModifiers.Fixed))
                return MapEntityPhysicsBehavior.FixedInPlaceWithoutCapsuleCollider;
            return Modifiers.HasFlag(MapEntityModifiers.FxdCol)
                ? MapEntityPhysicsBehavior.FixedInPlaceWithCapsuleCollider
                : MapEntityPhysicsBehavior.Normal;
        }
        set
        {
            switch (value)
            {
                case MapEntityPhysicsBehavior.Normal:
                    Modifiers &= ~MapEntityModifiers.Fixed;
                    Modifiers &= ~MapEntityModifiers.FxdCol;
                    break;
                case MapEntityPhysicsBehavior.FixedInPlaceWithoutCapsuleCollider:
                    Modifiers |= MapEntityModifiers.Fixed;
                    Modifiers &= ~MapEntityModifiers.FxdCol;
                    break;
                case MapEntityPhysicsBehavior.FixedInPlaceWithCapsuleCollider:
                    Modifiers &= ~MapEntityModifiers.Fixed;
                    Modifiers |= MapEntityModifiers.FxdCol;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(PhysicsBehavior));
                    break;
            }
        }
    }

    public bool IsActiveEvenWhenOutOfCameraRange
    {
        get => Modifiers.HasFlag(MapEntityModifiers.ALW);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.ALW;
            else
                Modifiers &= ~MapEntityModifiers.ALW;
        }
    }

    public bool IsActiveEvenWhenGameIsPaused
    {
        get => Modifiers.HasFlag(MapEntityModifiers.PAU);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.PAU;
            else
                Modifiers &= ~MapEntityModifiers.PAU;
        }
    }

    public bool IsDisabledWhenCurrentInsideIsDifferent
    {
        get => Modifiers.HasFlag(MapEntityModifiers.HIDE);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.HIDE;
            else
                Modifiers &= ~MapEntityModifiers.HIDE;
        }
    }

    // TODO: Figure out a way to patch the game so this isn't needed
    public bool HasEulerAnglesSetWithDelayOnMapLoad
    {
        get => Modifiers.HasFlag(MapEntityModifiers.ROT);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.ROT;
            else
                Modifiers &= ~MapEntityModifiers.ROT;
        }
    }

    internal int[] OriginalRequires { get; } = new int[10];
    internal int[] OriginalLimits { get; } = new int[10];

    public List<Branch<FlagLeaf>> RequiredFlags { get; } = new();
    public List<LimitFlag> LimitedToFlags { get; } = new();

    public virtual Vector3 EntityStartingPosition
    {
        get => InternalStartingPosition;
        set => InternalStartingPosition = value;
    }

    public Vector3 TransformEulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }
    public Color TagColor { get => InternalTagColor; set => InternalTagColor = value; }

    public int? InsideId
    {
        get => InternalInsideId < 0 ? null : InternalInsideId;
        set => InternalInsideId = value ?? -1;
    }

    public int? RegionalFlagId
    {
        get => InternalRegionalFlagId < 0 ? null : InternalRegionalFlagId;
        set => InternalRegionalFlagId = value ?? -1;
    }

    internal bool IsReturnToHeightOriginallyInt { get; set; }
    internal bool InternalReturnToHeight { get; set; } = true;
    internal int InternalInsideId { get; set; } = -1;
    internal Color InternalTagColor { get; set; }

    internal NPCControl.ActionBehaviors InternalOutOfRangeBehavior { get; set; }
    internal float InternalOutOfRangeActionFrequency { get; set; } = 200f;
    internal NPCControl.ActionBehaviors InternalInRangeBehavior { get; set; }
    internal float InternalInRangeActionFrequency { get; set; } = 200f;

    internal NPCControl.DeathType InternalDeathType { get; set; }
    internal int InternalAnimIdOrItemId { get; set; } = -1;
    internal bool InternalIsFlipped { get; set; }
    internal Vector3 InternalStartingPosition { get; set; }
    internal float InternalInitialHeight { get; set; }
    internal float InternalBobRange { get; set; }
    internal float InternalBobSpeed { get; set; }

    internal float InternalCcolHeight { get; set; } = 2f;
    internal float InternalCcolRadius { get; set; } = 0.5f;

    internal Vector3 InternalEulerAngles { get; set; }
    internal float InternalRadius { get; set; }
    internal float InternalTimer { get; set; } = -1f;

    internal float InternalSpeed { get; set; } = 5f;

    internal float InternalSpeedMultiplier { get; set; } = 1f;
    internal float InternalRadiusLimit { get; set; } = 6f;
    internal float InternalWanderRadius { get; set; } = 3f;
    internal float InternalTeleportRadius { get; set; } = 9f;

    internal float InternalFreezeTime { get; set; } = 600f;

    // TODO: These 2 fields don't work because CheckSpecialID will not honor them, but if they are specified meaning they
    // TODO: have a magnitude above 0.1f, it will still cause the offset to be overriden to (0f, 1f, 0f) and the size to (2f, 2f, 1f)
    // TODO: This can't work for modding so we have to retroactively change all existing ones to these values and then patch
    // TODO: the game to remove this quirk so they can actually function as it's supposed to
    internal Vector3 InternalFreezeSize { get; set; } = Vector3.one;
    internal Vector3 InternalFreezeOffset { get; set; } = Vector3.zero;

    internal int InternalActivationFlagId { get; set; } = -1;
    internal int InternalRegionalFlagId { get; set; } = -1;

    internal int InternalSpyDialogueId { get; set; } = -1;
    internal int InternalEventId { get; set; } = -1;

    internal bool InternalHaxBoxCol { get; set; }
    internal bool InternalBoxColIsTrigger { get; set; }
    internal Vector3 InternalBoxColSize { get; set; } = Vector3.one;
    internal Vector3 InternalBoxColCenter { get; set; }

    internal int[] OriginalData { get; } = new int[10];
    internal List<Ref<int>> InternalData { get; } = new();

    internal Vector3[] OriginalVectorData { get; } = new Vector3[10];
    internal List<Ref<Vector3>> InternalVectorData { get; } = new();
    internal Vector3[] InternalSecondaryVectorDataArray { get; set; } = [];
    internal List<Ref<Vector3>> InternalSecondaryVectorData { get; } = new();

    internal Vector3[] OriginalDialogues { get; } = new Vector3[20];
    internal List<Ref<Vector3>> InternalDialogues { get; } = new();

    internal int[] OriginalBattleEnemyIds { get; } = new int[4];
    internal List<Ref<int>> InternalBattleEnemyIds { get; } = new();

    internal Vector3 InternalEmoticonOffset { get; set; } = Vector3.zero;
    internal Vector2[] OriginalEmoticonFlags { get; } = Enumerable.Repeat(new Vector2(-1, 0), 10).ToArray();
    internal List<Ref<Vector2>> InternalEmoticonFlags { get; } = new();

    internal string UnusedOverflowData { get; set; } = "";
}