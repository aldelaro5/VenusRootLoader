using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DigSpots;

public sealed class DigSpotMedalMapEntityLeaf : DigSpotMapEntityLeaf
{
    internal DigSpotMedalMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<MedalLeaf> MedalHiddenInside
    {
        get;
        set
        {
            InternalData[2].Value = value.GameId;
            field = value;
        }
    }

    public Branch<FlagLeaf>? FlagSetToTrueWhenCollectingMedal
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition, Branch<MedalLeaf> medalHiddenInside)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(0), new(2), new(0)]);
        MedalHiddenInside = medalHiddenInside;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<MedalLeaf> medalsRegistry = registryResolver.Resolve<MedalLeaf>();
        MedalHiddenInside = new(medalsRegistry.LeavesByGameIds[InternalData[2].Value]);

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        FlagSetToTrueWhenCollectingMedal = InternalActivationFlagId switch
        {
            > 0 => new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]),
            _ => FlagSetToTrueWhenCollectingMedal
        };
    }
}