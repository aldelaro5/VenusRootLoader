using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Collectibles;

public sealed class CollectibleItemMapEntityLeaf : CollectibleMapEntityLeaf
{
    internal CollectibleItemMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<ItemLeaf> Item
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public bool IsKeyItem
    {
        get => InternalData[0].Value == 1;
        set => InternalData[0].Value = value ? 1 : 0;
    }

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal void InitializeFromNew(Vector3 startingPosition, Branch<ItemLeaf> item, bool isKeyItem)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(0), new(-1), new(0)]);
        Item = item;
        IsKeyItem = isKeyItem;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        if (InternalData.Count < 2)
            InternalData.Add(new(-1));
        if (InternalData.Count < 3)
            InternalData.Add(new(0));

        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        Item = new(itemsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}