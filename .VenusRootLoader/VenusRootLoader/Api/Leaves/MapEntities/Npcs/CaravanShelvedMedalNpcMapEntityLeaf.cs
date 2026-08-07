using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class CaravanShelvedMedalNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal CaravanShelvedMedalNpcMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.CaravanBadge;

    public Branch<ItemShopMapEntityLeaf> AssociatedItemShop
    {
        get;
        set
        {
            if (value.Resolve().Map != Map)
            {
                ThrowHelper.ThrowInvalidOperationException(
                    $"The associated items shop must be in the {Map.NamedId} map");
            }

            InternalData[0].Value = value.Resolve().GameId;
            field = value;
        }
    }

    public Branch<DialogueLeaf> ShopKeeperDialogueWhenInteracting
    {
        get;
        set
        {
            if (value.Resolve().AssociatedMap is not null && value.Resolve().AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalData[1].Value = value.Resolve().GameId;
            field = value;
        }
    }

    public NpcHornInteraction HornInteraction
    {
        get
        {
            if (Modifiers.HasFlag(MapEntityModifiers.ITHD))
                return NpcHornInteraction.InteractWithHornDashOnly;
            return Modifiers.HasFlag(MapEntityModifiers.ITAH)
                ? NpcHornInteraction.InteractWithAnyHornAttack
                : NpcHornInteraction.None;
        }
        set
        {
            switch (value)
            {
                case NpcHornInteraction.None:
                    Modifiers &= ~MapEntityModifiers.ITAH;
                    Modifiers &= ~MapEntityModifiers.ITHD;
                    break;
                case NpcHornInteraction.InteractWithHornDashOnly:
                    Modifiers &= ~MapEntityModifiers.ITAH;
                    Modifiers |= MapEntityModifiers.ITHD;
                    break;
                case NpcHornInteraction.InteractWithAnyHornAttack:
                    Modifiers |= MapEntityModifiers.ITAH;
                    Modifiers &= ~MapEntityModifiers.ITHD;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(PhysicsBehavior));
                    break;
            }
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<ItemShopMapEntityLeaf> associatedItemShop,
        Branch<DialogueLeaf> shopKeeperDialogueWhenInteracting)
    {
        base.InitializeFromNew(startingPosition, null);
        for (int i = 0; i < 2; i++)
            InternalData.Add(new Ref<int>(0));
        AssociatedItemShop = associatedItemShop;
        ShopKeeperDialogueWhenInteracting = shopKeeperDialogueWhenInteracting;
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = RegistryResolver.Resolve<CommonDialogueLeaf>();

        AssociatedItemShop =
            (ItemShopMapEntityLeaf)Map.Resolve().EntitiesRegistry.GetByGameId(InternalData[0].Value);
        ShopKeeperDialogueWhenInteracting = InternalData[1].Value < 0
            ? commonDialoguesRegistry.GetByGameId(InternalData[1].Value)
            : Map.Resolve().DialoguesRegistry.GetByGameId(InternalData[1].Value);
    }
}