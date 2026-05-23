using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities.ActionBehaviors;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Npcs;

using ShelvedItemForSale = (Branch<ItemLeaf> Item, Vector3 Position);

public sealed class ItemsShopMapEntity : MapEntity
{
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

    public int DialogueIdWhenInteractingWithShopKeeper
    {
        get => (int)InternalDialogues[0].y;
        set => InternalDialogues[0] = new(InternalDialogues[0].x, value, InternalDialogues[0].z);
    }

    public float? ItemsBuyingPriceMultiplier
    {
        get => InternalDialogues[2].y > 0.1f ? InternalDialogues[2].y / 10f : null;
        set => InternalDialogues[2] = new(InternalDialogues[2].x, value * 10f ?? 0f, InternalDialogues[2].z);
    }

    public int DialogueIdWhenInteractingWithShelvedItem
    {
        get => (int)InternalDialogues[6].y;
        set => InternalDialogues[6] = new(InternalDialogues[6].x, value, InternalDialogues[6].z);
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

    internal ItemsShopMapEntity()
    {
        Behaviors = new(this);
    }

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = 0;
        InternalDialogues.AddRange(Enumerable.Repeat(Vector3.zero, 11));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();

        Behaviors.InitializeBehaviorFromExisting(registryResolver);

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        List<ShelvedItemForSale> itemsForSale =
            InternalData.Zip(
                    InternalVectorData,
                    (data, vectorData) =>
                        new ShelvedItemForSale(new(itemsRegistry.LeavesByGameIds[data]), vectorData))
                .ToList();
        ChangeItemsForSale(itemsForSale);
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