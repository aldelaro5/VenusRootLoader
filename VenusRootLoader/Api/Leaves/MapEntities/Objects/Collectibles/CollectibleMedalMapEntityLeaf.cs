using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Collectibles;

public sealed class CollectibleMedalMapEntityLeaf : CollectibleMapEntityLeaf
{
    internal CollectibleMedalMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<MedalLeaf> Medal
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
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

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition, Branch<MedalLeaf> medal)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(2), new(-1), new(0)]);
        Medal = medal;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        if (InternalData.Count < 2)
            InternalData.Add(new(-1));
        if (InternalData.Count < 3)
            InternalData.Add(new(0));

        ILeavesRegistry<MedalLeaf> medalsRegistry = registryResolver.Resolve<MedalLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        Medal = new(medalsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
        if (InternalActivationFlagId > 0)
            FlagSetToTrueWhenCollecting = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}