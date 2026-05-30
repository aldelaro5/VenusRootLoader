using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: Patch a proper fix to the ROT modifier workaround which doesn't work right and is basically a race condition
// TODO: Figure out if we can make this work for NPCs which seems to not lock their rigid properly
public sealed class TrappedEntityLeafMapEntityLeaf : MapEntityLeaf
{
    internal TrappedEntityLeafMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.CoiledObject;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public int TrappedMapEntityId { get => InternalData[0]; set => InternalData[0] = value; }

    public Branch<FlagLeaf>? FlagSetWhenEntityGetsUntrapped
    {
        get;
        set
        {
            InternalData[1] = value?.GameId ?? -1;
            field = value;
        }
    }

    public int? RegionalFlagIdSetWhenEntityGetsUntrapped
    {
        get => InternalRegionalFlagId < 0 ? null : InternalRegionalFlagId;
        set => InternalRegionalFlagId = value ?? -1;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, -1]);
        InternalVectorData.Add(new(0f, -11.0f, 0f));
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.CoilyVine - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(2.5f, 5f, 2.5f);
        InternalBoxColCenter = new(0f, -11.5f, 0f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 2)
            InternalData.AddRange(Enumerable.Repeat(-1, 2 - InternalData.Count));

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        if (InternalData[1] > -1)
            FlagSetWhenEntityGetsUntrapped = new(flagsRegistry.LeavesByGameIds[InternalData[1]]);
    }
}