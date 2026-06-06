using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
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

            InternalDialogues[0].Value.y = value.GameId;
            field = value;
        }
    }

    public float? ItemsBuyingPriceMultiplier
    {
        get => InternalDialogues[2].Value.y > 0.1f ? InternalDialogues[2].Value.y / 10f : null;
        set => InternalDialogues[2].Value.y = value * 10f ?? 0f;
    }

    public Branch<DialogueLeaf> DialogueWhenInteractingWithShelvedItem
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalDialogues[6].Value.y = value.GameId;
            field = value;
        }
    }

    public float? ShelvedItemsInteractionRadius
    {
        get => InternalDialogues[8].Value.x > 0.1f ? InternalDialogues[8].Value.x / 10f : null;
        set => InternalDialogues[8].Value.x = value * 10f ?? 0f;
    }

    public ReadOnlyCollection<ItemShopShelvedItemForSale> ItemsForSale { get; private set; } =
        new List<ItemShopShelvedItemForSale>().AsReadOnly();

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalDialogues.AddRange(Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 11));
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
                        Item = itemsRegistry.LeavesByGameIds[data.Value],
                        Position = vectorData.Value
                    })
                .ToList();
        ChangeItemsForSale(itemsForSale);

        DialogueWhenInteractingWithShopKeeper = (int)InternalDialogues[0].Value.y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].Value.y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].Value.y];
        DialogueWhenInteractingWithShelvedItem = (int)InternalDialogues[6].Value.y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].Value.y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].Value.y];
    }

    public void ChangeItemsForSale(List<ItemShopShelvedItemForSale> itemsForSale)
    {
        InternalVectorData.Clear();
        InternalData.Clear();
        foreach (ItemShopShelvedItemForSale itemForSale in itemsForSale)
        {
            InternalData.Add(new(itemForSale.Item.GameId));
            InternalVectorData.Add(new(itemForSale.Position));
        }

        ItemsForSale = itemsForSale.AsReadOnly();
    }
}