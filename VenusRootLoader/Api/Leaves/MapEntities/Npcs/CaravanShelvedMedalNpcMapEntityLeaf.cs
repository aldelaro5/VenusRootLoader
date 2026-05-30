using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class CaravanShelvedMedalNpcMapEntityLeaf : MapEntityLeaf
{
    internal CaravanShelvedMedalNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.NPC;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.CaravanBadge;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public ItemsShopMapEntityLeaf AssociatedItemShop
    {
        get => (ItemsShopMapEntityLeaf)Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[0]];
        set
        {
            if (value.Map != Map)
            {
                ThrowHelper.ThrowInvalidOperationException(
                    "The associated items shop map entity must be on the same map as this NPC");
            }

            InternalData[0] = value.GameId;
        }
    }

    public int ShopKeeperDialogueIdWhenInteracting
    {
        get => InternalData[1];
        set => InternalData[1] = value;
    }

    public float InteractRangeRadius
    {
        get => InternalRadius;
        set => InternalRadius = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange(Enumerable.Repeat(0, 2 - InternalData.Count));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}