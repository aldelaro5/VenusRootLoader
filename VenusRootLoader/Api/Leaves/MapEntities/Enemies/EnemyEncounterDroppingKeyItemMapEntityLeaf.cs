using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyEncounterDroppingKeyItemMapEntityLeaf : EnemyEncounterMapEntityLeaf
{
    internal EnemyEncounterDroppingKeyItemMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal NPCControl.DeathType DeathMethod { get => InternalDeathType; set => InternalDeathType = value; }

    public Branch<ItemLeaf> KeyItemDroppedWhenDefeated
    {
        get;
        set
        {
            InternalVectorData[0].Value.x = value.GameId;
            field = value;
        }
    }

    public Branch<FlagLeaf> KeyItemObtainedFlag
    {
        get => Limits[0].Flag;
        set
        {
            InternalActivationFlagId = value.GameId;
            Limits[0].Flag = value;
        }
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalVectorData.Add(new(new(0f, -2f, 0f)));
        Limits.Add(
            new()
            {
                Flag = new(),
                FailsWholeConditionWhenFlagIsTrue = false
            });
        InternalActivationFlagId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();

        KeyItemDroppedWhenDefeated = new(itemsRegistry.LeavesByGameIds[(int)InternalVectorData[0].Value.x]);
    }
}