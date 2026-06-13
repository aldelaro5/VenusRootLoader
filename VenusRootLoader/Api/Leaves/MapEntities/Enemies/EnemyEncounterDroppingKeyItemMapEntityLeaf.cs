using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

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
        get => LimitedToFlags[0].Flag;
        set
        {
            InternalActivationFlagId = value.GameId;
            LimitedToFlags[0].Flag = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        IList<Branch<EnemyLeaf>> enemiesFormationInBattle,
        Branch<ItemLeaf> keyItemDroppedWhenDefeated,
        Branch<FlagLeaf> keyItemObtainedFlag)
    {
        base.InitializeFromNew(startingPosition, animId, enemiesFormationInBattle);
        InternalVectorData.Add(new(new(0f, -2f, 0f)));
        KeyItemDroppedWhenDefeated = keyItemDroppedWhenDefeated;
        LimitedToFlags.Add(
            new()
            {
                Flag = keyItemObtainedFlag,
                FailsWholeConditionWhenFlagIsTrue = false
            });
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();

        KeyItemDroppedWhenDefeated = new(itemsRegistry.LeavesByGameIds[(int)InternalVectorData[0].Value.x]);
    }
}