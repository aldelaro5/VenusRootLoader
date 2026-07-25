using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Enums;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Collectibles;

public sealed class CollectibleItemMapEntityLeaf : CollectibleMapEntityLeaf
{
    internal CollectibleItemMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<ItemLeaf> Item
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public bool IsAKeyItem
    {
        get => InternalData[0].Value == 1;
        set => InternalData[0].Value = value ? 1 : 0;
    }

    public Branch<FlagLeaf>? FlagSetToTrueWhenCollecting
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    public ObjectDetectorBehavior DetectorBehaviorIfKeyItemWithCollectionFlagSet
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
                    ThrowHelper.ThrowArgumentOutOfRangeException(
                        nameof(DetectorBehaviorIfKeyItemWithCollectionFlagSet));
                    break;
            }
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition, Branch<ItemLeaf> item, bool isKeyItem)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(0), new(-1), new(0)]);
        Item = item;
        IsAKeyItem = isKeyItem;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        if (InternalData.Count < 2)
            InternalData.Add(new(-1));
        if (InternalData.Count < 3)
            InternalData.Add(new(0));

        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        Item = new(itemsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
        if (InternalActivationFlagId > 0)
            FlagSetToTrueWhenCollecting = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}