using System.Collections.ObjectModel;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

using ItemDrop = (Branch<ItemLeaf> Item, Branch<FlagLeaf>? RequiredFlag);

public sealed class EnemyEncounterWithRegularItemDropsMapEntityLeaf : EnemyMapEntityLeaf
{
    internal EnemyEncounterWithRegularItemDropsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal NPCControl.DeathType DeathMethod { get => InternalDeathType; set => InternalDeathType = value; }

    public ReadOnlyCollection<ItemDrop> ItemsDropPoolWhenDefeated { get; private set; } =
        new List<ItemDrop>().AsReadOnly();

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        List<ItemDrop> itemsDrop = InternalVectorData
            .Select(itemDrop =>
            {
                Branch<ItemLeaf> item = new(itemsRegistry.LeavesByGameIds[(int)itemDrop.x]);
                Branch<FlagLeaf>? flag = itemDrop.y switch
                {
                    >= 0f => new(flagsRegistry.LeavesByGameIds[(int)itemDrop.y]),
                    _ => null
                };
                return (item, flag);
            })
            .ToList();
        ChangeItemsDropPoolWhenDefeated(itemsDrop);
    }

    public void ChangeItemsDropPoolWhenDefeated(List<ItemDrop> items)
    {
        InternalVectorData.Clear();
        foreach (ItemDrop itemDrop in items)
        {
            int itemGameId = itemDrop.Item.GameId;
            int requiredFlagGameId = itemDrop.RequiredFlag?.GameId ?? -1;
            InternalVectorData.Add(new(itemGameId, requiredFlagGameId, 0f));
        }

        ItemsDropPoolWhenDefeated = items.AsReadOnly();
    }
}