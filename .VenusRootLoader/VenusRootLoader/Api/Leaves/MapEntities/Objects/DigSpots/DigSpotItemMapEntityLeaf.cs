using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Enums;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DigSpots;

public sealed class DigSpotItemMapEntityLeaf : DigSpotMapEntityLeaf
{
    internal DigSpotItemMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<ItemLeaf> ItemHiddenInside
    {
        get;
        set
        {
            InternalData[2].Value = value.GameId;
            field = value;
        }
    }

    public bool IsHiddenItemAKeyItem
    {
        get => InternalData[1].Value == 1;
        set => InternalData[1].Value = value ? 1 : 0;
    }

    public Branch<FlagLeaf>? FlagSetToTrueWhenCollectingItem
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    // TODO: Mention in the xmldoc it only applies to Lore Book in base game
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

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<ItemLeaf> itemHiddenInside,
        bool isHiddenItemAKeyItem)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(0), new(0), new(0)]);
        ItemHiddenInside = itemHiddenInside;
        IsHiddenItemAKeyItem = isHiddenItemAKeyItem;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ItemHiddenInside = new(itemsRegistry.LeavesByGameIds[InternalData[2].Value]);

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        FlagSetToTrueWhenCollectingItem = InternalActivationFlagId switch
        {
            > 0 => new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]),
            _ => FlagSetToTrueWhenCollectingItem
        };
    }
}