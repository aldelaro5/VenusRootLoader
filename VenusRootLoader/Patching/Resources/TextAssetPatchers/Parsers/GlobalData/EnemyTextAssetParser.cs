using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class EnemyTextAssetParser : ITextAssetParser<EnemyLeaf>
{
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    public EnemyTextAssetParser(ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _enemiesRegistry = enemiesRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, EnemyLeaf value)
    {
        StringBuilder sb = new();

        sb.Append(value.EntityAnimId);
        sb.Append(',');
        sb.Append(value.BaseMaxHp);
        sb.Append(',');
        sb.Append(value.BaseDefense);
        sb.Append(',');
        sb.Append(value.BaseExpReward);
        sb.Append(',');
        sb.Append(value.BaseBerriesDropAmount);
        sb.Append(',');
        sb.Append(value.CursorOffset.x);
        sb.Append(',');
        sb.Append(value.CursorOffset.y);
        sb.Append(',');
        sb.Append(value.CursorOffset.z);
        sb.Append(',');
        sb.Append(value.PoisonResistance);
        sb.Append(',');
        sb.Append(value.FreezeResistance);
        sb.Append(',');
        sb.Append(value.NumbResistance);
        sb.Append(',');
        sb.Append(value.SleepResistance);
        sb.Append(',');
        sb.Append(value.LogicalSize);
        sb.Append(',');
        sb.Append(value.EntityFreezeSize.x);
        sb.Append(',');
        sb.Append(value.EntityFreezeSize.y);
        sb.Append(',');
        sb.Append(value.EntityFreezeSize.z);
        sb.Append(',');
        sb.Append(value.EntityFreezeOffset.x);
        sb.Append(',');
        sb.Append(value.EntityFreezeOffset.y);
        sb.Append(',');
        sb.Append(value.EntityFreezeOffset.z);
        sb.Append(',');
        sb.Append(value.StartingBattlePosition.ToString());
        sb.Append(',');
        sb.Append(value.EntityHeight);
        sb.Append(',');
        sb.Append(value.EntityBobSpeed);
        sb.Append(',');
        sb.Append(value.EntityBobRange);
        sb.Append(',');

        sb.Append(value.Properties.Count);
        foreach (BattleControl.AttackProperty property in value.Properties)
        {
            sb.Append('{');
            sb.Append(property.ToString());
        }

        sb.Append("{,");

        sb.Append(value.Weight);
        sb.Append(',');
        sb.Append(value.BaseEnemyId?.GameId ?? -1);
        sb.Append(',');
        sb.Append(value.EventIdOnDeath ?? -1);
        sb.Append(',');
        sb.Append(value.ActorTurnAmountPerMainTurn);
        sb.Append(',');
        sb.Append(!value.CanBeTaunted);
        sb.Append(',');
        sb.Append(!value.CanFall);
        sb.Append(',');
        sb.Append(value.HasFixedExpScaling);
        sb.Append(',');
        sb.Append(!value.IsAffectedByExhaustion);
        sb.Append(',');
        sb.Append(value.HasStatsHiddenFromHud);
        sb.Append(',');
        sb.Append(value.InternalDeathType);
        sb.Append(',');

        List<Branch<EnemyLeaf>> enemies = value.EnemiesWhoTriggerHitActionWhenDamaged;
        if (enemies.Count == 0)
            sb.Append("-1");
        else
            sb.Append(string.Join(";", enemies.Select(enemy => enemy.GameId.ToString())));

        sb.Append(',');
        sb.Append(value.HardModeAttackIncrease);
        sb.Append(',');
        sb.Append(value.HardModeBaseMaxHpIncrease);
        sb.Append(',');
        sb.Append(value.HardModeBaseDefenseIncrease);
        sb.Append(',');
        sb.Append(value.DefenseIncreaseWhenDefending);
        sb.Append(',');
        sb.Append(value.ItemOffset.x);
        sb.Append(',');
        sb.Append(value.ItemOffset.y);
        sb.Append(',');
        sb.Append(value.ItemOffset.z);
        sb.Append(',');
        sb.Append(value.IsBaseStateBattleIdle);
        sb.Append(',');
        int? enemyPortraitsSpriteIndex = ((IEnemyPortraitSprite)value).EnemyPortraitsSpriteIndex;
        sb.Append(enemyPortraitsSpriteIndex ?? -1);
        sb.Append(',');
        sb.Append(!value.CanBeSpied);
        sb.Append(',');
        sb.Append(value.EventIdOnFall ?? -1);
        sb.Append(',');
        sb.Append((int)value.HitActionTrigger);
        sb.Append(',');
        sb.Append(value.CanActWhileStunned);
        sb.Append(',');
        sb.Append(value.SizeWhenFrozen);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, EnemyLeaf value)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        IDictionary<int, EnemyLeaf> enemiesByGameId = _enemiesRegistry.LeavesByGameIds;

        value.EntityAnimId = int.Parse(fields[0]);
        value.BaseMaxHp = int.Parse(fields[1]);
        value.BaseDefense = int.Parse(fields[2]);
        value.BaseExpReward = int.Parse(fields[3]);
        value.BaseBerriesDropAmount = int.Parse(fields[4]);
        value.CursorOffset = new(float.Parse(fields[5]), float.Parse(fields[6]), float.Parse(fields[7]));
        value.PoisonResistance = int.Parse(fields[8]);
        value.FreezeResistance = int.Parse(fields[9]);
        value.NumbResistance = int.Parse(fields[10]);
        value.SleepResistance = int.Parse(fields[11]);
        value.LogicalSize = float.Parse(fields[12]);
        value.EntityFreezeSize = new(float.Parse(fields[13]), float.Parse(fields[14]), float.Parse(fields[15]));
        value.EntityFreezeOffset = new(float.Parse(fields[16]), float.Parse(fields[17]), float.Parse(fields[18]));
        value.StartingBattlePosition = Enum.Parse<BattleControl.BattlePosition>(fields[19]);
        value.EntityHeight = float.Parse(fields[20]);
        value.EntityBobSpeed = float.Parse(fields[21]);
        value.EntityBobRange = float.Parse(fields[22]);

        value.Properties.Clear();
        string[] properties = fields[23].Split(
            StringUtils.OpeningBraceSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < properties.Length; i++)
            value.Properties.Add(Enum.Parse<BattleControl.AttackProperty>(properties[i]));

        value.Weight = float.Parse(fields[24]);
        int baseEnemyId = int.Parse(fields[25]);
        value.BaseEnemyId = baseEnemyId < 0 ? null : new(enemiesByGameId[baseEnemyId]);
        int eventIdOnDeath = int.Parse(fields[26]);
        value.EventIdOnDeath = eventIdOnDeath == 1 ? -1 : eventIdOnDeath;
        value.ActorTurnAmountPerMainTurn = int.Parse(fields[27]);
        value.CanBeTaunted = !bool.Parse(fields[28]);
        value.CanFall = !bool.Parse(fields[29]);
        value.HasFixedExpScaling = bool.Parse(fields[30]);
        value.IsAffectedByExhaustion = !bool.Parse(fields[31]);
        value.HasStatsHiddenFromHud = bool.Parse(fields[32]);
        value.InternalDeathType = int.Parse(fields[33]);

        value.EnemiesWhoTriggerHitActionWhenDamaged.Clear();
        if (!string.IsNullOrWhiteSpace(fields[34]) && fields[34] != "-1")
        {
            string[] enemyIds = fields[34].Split(StringUtils.SemiColonSplitDelimiter);
            foreach (string enemyId in enemyIds)
                value.EnemiesWhoTriggerHitActionWhenDamaged.Add(new(enemiesByGameId[int.Parse(enemyId)]));
        }

        value.HardModeAttackIncrease = int.Parse(fields[35]);
        value.HardModeBaseMaxHpIncrease = int.Parse(fields[36]);
        value.HardModeBaseDefenseIncrease = int.Parse(fields[37]);
        value.DefenseIncreaseWhenDefending = int.Parse(fields[38]);
        value.ItemOffset = new(float.Parse(fields[39]), float.Parse(fields[40]), float.Parse(fields[41]));
        value.IsBaseStateBattleIdle = bool.Parse(fields[42]);
        ((IEnemyPortraitSprite)value).EnemyPortraitsSpriteIndex = int.Parse(fields[43]);
        value.CanBeSpied = !bool.Parse(fields[44]);
        int eventIdOnFall = int.Parse(fields[45]);
        value.EventIdOnFall = eventIdOnFall == -1 ? null : eventIdOnFall;
        value.HitActionTrigger = (EnemyLeaf.AutoHitActionTrigger)int.Parse(fields[46]);
        value.CanActWhileStunned = bool.Parse(fields[47]);
        value.SizeWhenFrozen = float.Parse(fields[48]);
    }
}