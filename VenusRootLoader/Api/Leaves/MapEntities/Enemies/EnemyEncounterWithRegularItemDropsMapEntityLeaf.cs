using System.Collections.ObjectModel;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyEncounterWithRegularItemDropsMapEntityLeaf : EnemyMapEntityLeaf
{
    internal EnemyEncounterWithRegularItemDropsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal NPCControl.DeathType DeathMethod { get => InternalDeathType; set => InternalDeathType = value; }

    public ReadOnlyCollection<EnemyItemDrop> ItemsDropPoolWhenDefeated { get; private set; } =
        new List<EnemyItemDrop>().AsReadOnly();

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        List<EnemyItemDrop> itemsDrop = InternalVectorData
            .Select(itemDrop =>
            {
                return new EnemyItemDrop
                {
                    Item = itemsRegistry.LeavesByGameIds[(int)itemDrop.x],
                    RequiredFlag = itemDrop.y switch
                    {
                        >= 0f => new(flagsRegistry.LeavesByGameIds[(int)itemDrop.y]),
                        _ => null
                    }
                };
            })
            .ToList();
        ChangeItemsDropPoolWhenDefeated(itemsDrop);
    }

    public void ChangeItemsDropPoolWhenDefeated(List<EnemyItemDrop> items)
    {
        InternalVectorData.Clear();
        foreach (EnemyItemDrop itemDrop in items)
        {
            int itemGameId = itemDrop.Item.GameId;
            int requiredFlagGameId = itemDrop.RequiredFlag?.GameId ?? -1;
            InternalVectorData.Add(new(itemGameId, requiredFlagGameId, 0f));
        }

        ItemsDropPoolWhenDefeated = items.AsReadOnly();
    }
}