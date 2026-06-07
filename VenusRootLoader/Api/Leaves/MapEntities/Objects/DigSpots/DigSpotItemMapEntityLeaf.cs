using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DigSpots;

public sealed class DigSpotItemMapEntityLeaf : MapEntityLeaf
{
    internal DigSpotItemMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DigSpot;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

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
        InternalData.AddRange([new(0), new(0), new(0)]);
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

        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ItemHiddenInside = new(itemsRegistry.LeavesByGameIds[InternalData[2].Value]);

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}