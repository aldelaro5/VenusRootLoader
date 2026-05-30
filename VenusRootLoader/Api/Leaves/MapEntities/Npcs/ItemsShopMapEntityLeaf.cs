using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

using ShelvedItemForSale = (Branch<ItemLeaf> Item, Vector3 Position);

public sealed class ItemsShopMapEntityLeaf : MapEntityLeaf
{
    internal ItemsShopMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        Behaviors = new(this);
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.NPC;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.Shop;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

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

    public ReadOnlyCollection<ShelvedItemForSale> ItemsForSale { get; private set; } =
        new List<ShelvedItemForSale>().AsReadOnly();

    public float MovementRadius
    {
        get => InternalRadiusLimit;
        set => InternalRadiusLimit = value;
    }

    public float BehaviorAndInteractRangeRadius
    {
        get => InternalRadius;
        set => InternalRadius = value;
    }

    public MapEntityBehaviors Behaviors { get; }

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = 0;
        InternalDialogues.AddRange(Enumerable.Repeat(Vector3.zero, 11));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        Behaviors.InitializeBehaviorFromExisting(registryResolver);

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        List<ShelvedItemForSale> itemsForSale =
            InternalData.Zip(
                    InternalVectorData,
                    (data, vectorData) =>
                        new ShelvedItemForSale(new(itemsRegistry.LeavesByGameIds[data]), vectorData))
                .ToList();
        ChangeItemsForSale(itemsForSale);

        DialogueWhenInteractingWithShopKeeper = (int)InternalDialogues[0].y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].y];
        DialogueWhenInteractingWithShelvedItem = (int)InternalDialogues[6].y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].y];
    }

    public void ChangeItemsForSale(List<ShelvedItemForSale> itemsForSale)
    {
        InternalVectorData.Clear();
        InternalData.Clear();
        foreach (ShelvedItemForSale itemForSale in itemsForSale)
        {
            InternalData.Add(itemForSale.Item.GameId);
            InternalVectorData.Add(itemForSale.Position);
        }

        ItemsForSale = itemsForSale.AsReadOnly();
    }
}