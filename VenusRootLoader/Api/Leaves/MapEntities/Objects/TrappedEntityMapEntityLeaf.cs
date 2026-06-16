using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: Patch a proper fix to the ROT modifier workaround which doesn't work right and is basically a race condition
// TODO: Figure out if we can make this work for NPCs which seems to not lock their rigid properly
// TODO: Remember to specify in the xmldoc that the regional gets set when untrapped
public sealed class TrappedEntityMapEntityLeaf : ObjectMapEntityLeaf
{
    internal TrappedEntityMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.CoiledObject;

    public Branch<MapEntityLeaf> TrappedMapEntity
    {
        get;
        set
        {
            InternalData[0].Value = value.GameId;
            field = value;
        }
    }

    public Branch<FlagLeaf>? FlagSetWhenEntityGetsUntrapped
    {
        get;
        set
        {
            InternalData[1].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<MapEntityLeaf> trappedMapEntity)
    {
        InternalData.AddRange([new(0), new(-1)]);
        InternalVectorData.Add(new(new(0f, -11.0f, 0f)));
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.CoilyVine - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(2.5f, 5f, 2.5f);
        InternalBoxColCenter = new(0f, -11.5f, 0f);
        TrappedMapEntity = trappedMapEntity;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 2)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(-1), 2 - InternalData.Count));

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        if (InternalData[1].Value > -1)
            FlagSetWhenEntityGetsUntrapped = new(flagsRegistry.LeavesByGameIds[InternalData[1].Value]);

        TrappedMapEntity = Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[0].Value];
    }
}