using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Enums;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Collectibles;

public sealed class CollectibleMedalMapEntityLeaf : CollectibleMapEntityLeaf
{
    internal CollectibleMedalMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public Branch<MedalLeaf> Medal
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.Resolve().GameId;
            field = value;
        }
    } = null!;

    public Branch<FlagLeaf>? FlagSetToTrueWhenCollecting
    {
        get;
        set
        {
            InternalActivationFlagId = value?.Resolve().GameId ?? -1;
            field = value;
        }
    }

    public ObjectDetectorBehavior DetectorBehaviorIfCollectionFlagSet
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
                        nameof(DetectorBehaviorIfCollectionFlagSet));
                    break;
            }
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition, Branch<MedalLeaf> medal)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(2), new(-1), new(0)]);
        Medal = medal;
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        if (InternalData.Count < 2)
            InternalData.Add(new(-1));
        if (InternalData.Count < 3)
            InternalData.Add(new(0));

        ILeavesRegistry<MedalLeaf> medalsRegistry = RegistryResolver.Resolve<MedalLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = RegistryResolver.Resolve<FlagLeaf>();

        Medal = new(medalsRegistry.GetByGameId(InternalAnimIdOrItemId));
        if (InternalActivationFlagId > 0)
            FlagSetToTrueWhenCollecting = new(flagsRegistry.GetByGameId(InternalActivationFlagId));
    }
}