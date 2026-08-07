using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class ItemShopMapEntityLeaf : NpcWithSpyDialogueMapEntityLeaf
{
    internal ItemShopMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
        _itemsForSale = new(InternalData, InternalVectorData, 0, x => x.RefItemGameId, x => x.RefPosition);
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.Shop;

    public Branch<DialogueLeaf> DialogueWhenInteractingWithShopKeeper
    {
        get;
        set
        {
            if (value.Resolve().AssociatedMap is not null && value.Resolve().AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalDialogues[0].Value.y = value.Resolve().GameId;
            field = value;
        }
    } = null!;

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
            if (value.Resolve().AssociatedMap is not null && value.Resolve().AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalDialogues[6].Value.y = value.Resolve().GameId;
            field = value;
        }
    } = null!;

    public float? ShelvedItemsInteractionRadius
    {
        get => InternalDialogues[8].Value.x > 0.1f ? InternalDialogues[8].Value.x / 10f : null;
        set => InternalDialogues[8].Value.x = value * 10f ?? 0f;
    }

    private readonly ListDoubleRefWrapper<ItemShopShelvedItemForSale, int, Vector3> _itemsForSale;
    public IList<ItemShopShelvedItemForSale> ItemsForSale => _itemsForSale;

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        Branch<DialogueLeaf>? spyDialogue,
        Branch<DialogueLeaf> dialogueWhenInteractingWithShopKeeper,
        Branch<DialogueLeaf> dialogueWhenInteractingWithShelvedItem,
        IList<ItemShopShelvedItemForSale> itemsForSale)
    {
        base.InitializeFromNew(startingPosition, animId, spyDialogue);
        for (int i = 0; i < 11; i++)
            InternalDialogues.Add(new Ref<Vector3>(Vector3.zero));
        DialogueWhenInteractingWithShopKeeper = dialogueWhenInteractingWithShopKeeper;
        DialogueWhenInteractingWithShelvedItem = dialogueWhenInteractingWithShelvedItem;
        foreach (ItemShopShelvedItemForSale itemForSale in itemsForSale)
            ItemsForSale.Add(itemForSale);
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        ILeavesRegistry<ItemLeaf> itemsRegistry = RegistryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = RegistryResolver.Resolve<CommonDialogueLeaf>();

        _itemsForSale.SynchronizeFromExistingData(
            InternalData.Zip(
                    InternalVectorData,
                    (data, vectorData) => new ItemShopShelvedItemForSale
                    {
                        Item = itemsRegistry.GetByGameId(data.Value),
                        Position = vectorData.Value
                    })
                .ToList());

        DialogueWhenInteractingWithShopKeeper = (int)InternalDialogues[0].Value.y < 0
            ? commonDialoguesRegistry.GetByGameId((int)InternalDialogues[0].Value.y)
            : Map.Resolve().DialoguesRegistry.GetByGameId((int)InternalDialogues[0].Value.y);
        DialogueWhenInteractingWithShelvedItem = (int)InternalDialogues[6].Value.y < 0
            ? commonDialoguesRegistry.GetByGameId((int)InternalDialogues[6].Value.y)
            : Map.Resolve().DialoguesRegistry.GetByGameId((int)InternalDialogues[6].Value.y);
    }
}