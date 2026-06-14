using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Enums;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Collectibles;

public abstract class CollectibleMapEntityLeaf : ObjectMapEntityLeaf
{
    protected CollectibleMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Item;

    public Branch<EventLeaf>? EventToStartWhenCollected
    {
        get;
        set
        {
            InternalData[1].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    public bool IsCatchableByBeemerang
    {
        get => InternalData[2].Value == 0;
        set => InternalData[2].Value = value ? 0 : 1;
    }

    public ObjectDetectorBehavior DetectorBehavior
    {
        get
        {
            if (Modifiers.HasFlag(MapEntityModifiers.NDTCT))
                return ObjectDetectorBehavior.NeverDetects;
            return Modifiers.HasFlag(MapEntityModifiers.DDIST)
                ? ObjectDetectorBehavior.MustBe20UnitsOrLessToDetect
                : ObjectDetectorBehavior.AlwaysDetects;
        }
        set
        {
            switch (value)
            {
                case ObjectDetectorBehavior.AlwaysDetects:
                    Modifiers &= ~MapEntityModifiers.NDTCT;
                    Modifiers &= ~MapEntityModifiers.DDIST;
                    break;
                case ObjectDetectorBehavior.MustBe20UnitsOrLessToDetect:
                    Modifiers &= ~MapEntityModifiers.NDTCT;
                    Modifiers |= MapEntityModifiers.DDIST;
                    break;
                case ObjectDetectorBehavior.NeverDetects:
                    Modifiers |= MapEntityModifiers.NDTCT;
                    Modifiers &= ~MapEntityModifiers.DDIST;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(DetectorBehavior));
                    break;
            }
        }
    }

    protected void InitializeFromNew(Vector3 startingPosition)
    {
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        if (InternalData[1].Value > -1)
            EventToStartWhenCollected = new(eventsRegistry.LeavesByGameIds[InternalData[1].Value]);
    }
}