using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class DigSpotMedalMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DigSpot;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public Branch<MedalLeaf> MedalHiddenInside
    {
        get;
        set
        {
            InternalData[2] = value.GameId;
            field = value;
        }
    }

    public int? RegionalFlagId
    {
        get => InternalRegionalFlagId < 0 ? null : InternalRegionalFlagId;
        set => InternalRegionalFlagId = value ?? -1;
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

    internal DigSpotMedalMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 2, 0]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.DigMound - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1.4f, 1.4f, 1.4f);
        InternalBoxColCenter = Vector3.zero;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(-1, 3 - InternalData.Count));

        ILeavesRegistry<MedalLeaf> medalsRegistry = registryResolver.Resolve<MedalLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        MedalHiddenInside = new(medalsRegistry.LeavesByGameIds[InternalData[2]]);

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}