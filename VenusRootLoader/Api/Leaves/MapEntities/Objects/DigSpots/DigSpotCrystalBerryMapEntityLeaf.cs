using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DigSpots;

public sealed class DigSpotCrystalBerryMapEntityLeaf : MapEntityLeaf
{
    internal DigSpotCrystalBerryMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DigSpot;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public Branch<CrystalBerryLeaf> CrystalBerryHiddenInside
    {
        get;
        set
        {
            InternalData[1].Value = value.GameId;
            field = value;
        }
    }

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(1), new(0), new(-1)]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.DigMound - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1.4f, 1.4f, 1.4f);
        InternalBoxColCenter = Vector3.zero;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(-1), 3 - InternalData.Count));

        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = registryResolver.Resolve<CrystalBerryLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        CrystalBerryHiddenInside = new(crystalBerriesRegistry.LeavesByGameIds[InternalData[1].Value]);

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}