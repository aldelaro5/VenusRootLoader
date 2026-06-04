using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class ItemsShopMapEntityLeaf : SpyableNpcMapEntityLeaf
{
    internal ItemsShopMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.Shop;

    public Branch<DialogueLeaf> DialogueWhenInteractingWithShopKeeper
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalDialogues[0] = new(InternalDialogues[0].x, value.GameId, InternalDialogues[0].z);
            field = value;
        }
    }

    public float? ItemsBuyingPriceMultiplier
    {
        get => InternalDialogues[2].y > 0.1f ? InternalDialogues[2].y / 10f : null;
        set => InternalDialogues[2] = new(InternalDialogues[2].x, value * 10f ?? 0f, InternalDialogues[2].z);
    }

    public Branch<DialogueLeaf> DialogueWhenInteractingWithShelvedItem
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalDialogues[6] = new(InternalDialogues[6].x, value.GameId, InternalDialogues[6].z);
            field = value;
        }
    }

    public float? ShelvedItemsInteractionRadius
    {
        get => InternalDialogues[8].x > 0.1f ? InternalDialogues[8].x / 10f : null;
        set => InternalDialogues[8] = new(value * 10f ?? 0f, InternalDialogues[8].y, InternalDialogues[8].z);
    }

    public ReadOnlyCollection<ItemShopShelvedItemForSale> ItemsForSale { get; private set; } =
        new List<ItemShopShelvedItemForSale>().AsReadOnly();

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalDialogues.AddRange(Enumerable.Repeat(Vector3.zero, 11));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        List<ItemShopShelvedItemForSale> itemsForSale =
            InternalData.Zip(
                    InternalVectorData,
                    (data, vectorData) => new ItemShopShelvedItemForSale
                    {
                        Item = itemsRegistry.LeavesByGameIds[data],
                        Position = vectorData
                    })
                .ToList();
        ChangeItemsForSale(itemsForSale);

        DialogueWhenInteractingWithShopKeeper = (int)InternalDialogues[0].y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].y];
        DialogueWhenInteractingWithShelvedItem = (int)InternalDialogues[6].y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].y];
    }

    public void ChangeItemsForSale(List<ItemShopShelvedItemForSale> itemsForSale)
    {
        InternalVectorData.Clear();
        InternalData.Clear();
        foreach (ItemShopShelvedItemForSale itemForSale in itemsForSale)
        {
            InternalData.Add(itemForSale.Item.GameId);
            InternalVectorData.Add(itemForSale.Position);
        }

        ItemsForSale = itemsForSale.AsReadOnly();
    }
}