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

    public Branch<ItemsShopMapEntityLeaf> AssociatedItemShop
    {
        get;
        set
        {
            if (value.Leaf.Map != Map)
            {
                ThrowHelper.ThrowInvalidOperationException(
                    $"The associated items shop must be in the {Map.NamedId} map");
            }

            InternalData[0] = value.GameId;
            field = value;
        }
    }

    public Branch<DialogueLeaf> ShopKeeperDialogueWhenInteracting
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalData[1] = value.GameId;
            field = value;
        }
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

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();
        
        AssociatedItemShop = (ItemsShopMapEntityLeaf)Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[0]];
        ShopKeeperDialogueWhenInteracting = InternalData[1] < 0
            ? commonDialoguesRegistry.LeavesByGameIds[InternalData[1]]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalData[1]];
    }
}