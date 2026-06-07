using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Collectibles;

public abstract class CollectibleMapEntityLeaf : ObjectMapEntityLeaf
{
    protected CollectibleMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Item;

    public Branch<EventLeaf>? EventToTriggerWhenCollected
    {
        get;
        set
        {
            InternalData[1].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    public bool IsCatchableByBeemerang
    {
        get => InternalData[2].Value == 0;
        set => InternalData[2].Value = value ? 0 : 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        if (InternalData[1].Value > -1)
            EventToTriggerWhenCollected = new(eventsRegistry.LeavesByGameIds[InternalData[1].Value]);
    }
}