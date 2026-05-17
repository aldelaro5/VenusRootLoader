using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class EnemyEncounterHoldingKeyItemMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Enemy;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;

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

    public ReadOnlyCollection<Branch<EnemyLeaf>> EnemiesFormationInBattle { get; private set; } =
        new List<Branch<EnemyLeaf>>().AsReadOnly();

    public Branch<ItemLeaf> KeyItemDropped
    {
        get;
        set
        {
            InternalVectorData[0] = new(value.GameId, InternalVectorData[0].y, InternalVectorData[0].z);
            field = value;
        }
    }

    public Branch<FlagLeaf> KeyItemObtainedFlag
    {
        get => InternalLimits[0].Flag;
        set
        {
            InternalActivationFlagId = value.GameId;
            InternalLimits[0].Flag = value;
        }
    }

    public int? RespawnTimerInFrames
    {
        get => InternalEventId == 0 ? null : InternalEventId;
        set => InternalEventId = value ?? 0;
    }

    internal EnemyEncounterHoldingKeyItemMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalBattleEnemyIds.Add(0);
        InternalAnimIdOrItemId = 0;
        InternalVectorData.Add(new(0f, -2f, 0f));
        InternalLimits.Add(
            new()
            {
                Flag = new(),
                FailsWholeConditionWhenFlagIsTrue = false
            });
        InternalActivationFlagId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EnemyLeaf> enemiesRegistry = registryResolver.Resolve<EnemyLeaf>();
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
        KeyItemDropped = new(itemsRegistry.LeavesByGameIds[(int)InternalVectorData[0].x]);

        List<Branch<EnemyLeaf>> enemies = InternalBattleEnemyIds
            .Select(i => new Branch<EnemyLeaf>(enemiesRegistry.LeavesByGameIds[i]))
            .ToList();
        ChangeEnemiesFormationInBattle(enemies);
    }

    public void ChangeEnemiesFormationInBattle(List<Branch<EnemyLeaf>> enemies)
    {
        InternalBattleEnemyIds.Clear();
        InternalBattleEnemyIds.AddRange(enemies.Select(t => t.GameId));
        EnemiesFormationInBattle = enemies.AsReadOnly();
    }
}