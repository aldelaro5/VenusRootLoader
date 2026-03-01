using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.GlobalData;

internal sealed class EnemyTextAssetParser : ITextAssetParser<EnemyLeaf>
{
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    public EnemyTextAssetParser(ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _enemiesRegistry = enemiesRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, EnemyLeaf leaf)
    {
        StringBuilder sb = new();

        sb.Append(leaf.EntityAnimId);
        sb.Append(',');
        sb.Append(leaf.BaseMaxHp);
        sb.Append(',');
        sb.Append(leaf.BaseDefense);
        sb.Append(',');
        sb.Append(leaf.BaseExpReward);
        sb.Append(',');
        sb.Append(leaf.BaseBerriesDropAmount);
        sb.Append(',');
        sb.Append(leaf.CursorOffset.x);
        sb.Append(',');
        sb.Append(leaf.CursorOffset.y);
        sb.Append(',');
        sb.Append(leaf.CursorOffset.z);
        sb.Append(',');
        sb.Append(leaf.PoisonResistance);
        sb.Append(',');
        sb.Append(leaf.FreezeResistance);
        sb.Append(',');
        sb.Append(leaf.NumbResistance);
        sb.Append(',');
        sb.Append(leaf.SleepResistance);
        sb.Append(',');
        sb.Append(leaf.LogicalSize);
        sb.Append(',');
        sb.Append(leaf.EntityFreezeSize.x);
        sb.Append(',');
        sb.Append(leaf.EntityFreezeSize.y);
        sb.Append(',');
        sb.Append(leaf.EntityFreezeSize.z);
        sb.Append(',');
        sb.Append(leaf.EntityFreezeOffset.x);
        sb.Append(',');
        sb.Append(leaf.EntityFreezeOffset.y);
        sb.Append(',');
        sb.Append(leaf.EntityFreezeOffset.z);
        sb.Append(',');
        sb.Append(leaf.StartingBattlePosition.ToString());
        sb.Append(',');
        sb.Append(leaf.EntityHeight);
        sb.Append(',');
        sb.Append(leaf.EntityBobSpeed);
        sb.Append(',');
        sb.Append(leaf.EntityBobRange);
        sb.Append(',');

        sb.Append(leaf.Properties.Count);
        foreach (BattleControl.AttackProperty property in leaf.Properties)
        {
            sb.Append('{');
            sb.Append(property.ToString());
        }

        sb.Append("{,");

        sb.Append(leaf.Weight);
        sb.Append(',');
        sb.Append(leaf.BaseEnemyId?.GameId ?? -1);
        sb.Append(',');
        sb.Append(leaf.EventIdOnDeath ?? -1);
        sb.Append(',');
        sb.Append(leaf.ActorTurnAmountPerMainTurn);
        sb.Append(',');
        sb.Append(!leaf.CanBeTaunted);
        sb.Append(',');
        sb.Append(!leaf.CanFall);
        sb.Append(',');
        sb.Append(leaf.HasFixedExpScaling);
        sb.Append(',');
        sb.Append(!leaf.IsAffectedByExhaustion);
        sb.Append(',');
        sb.Append(leaf.HasStatsHiddenFromHud);
        sb.Append(',');
        sb.Append(leaf.InternalDeathType);
        sb.Append(',');

        List<Branch<EnemyLeaf>> enemies = leaf.EnemiesWhoTriggerHitActionWhenDamaged;
        if (enemies.Count == 0)
            sb.Append("-1");
        else
            sb.Append(string.Join(";", enemies.Select(enemy => enemy.GameId.ToString())));

        sb.Append(',');
        sb.Append(leaf.HardModeAttackIncrease);
        sb.Append(',');
        sb.Append(leaf.HardModeBaseMaxHpIncrease);
        sb.Append(',');
        sb.Append(leaf.HardModeBaseDefenseIncrease);
        sb.Append(',');
        sb.Append(leaf.DefenseIncreaseWhenDefending);
        sb.Append(',');
        sb.Append(leaf.ItemOffset.x);
        sb.Append(',');
        sb.Append(leaf.ItemOffset.y);
        sb.Append(',');
        sb.Append(leaf.ItemOffset.z);
        sb.Append(',');
        sb.Append(leaf.IsBaseStateBattleIdle);
        sb.Append(',');
        int? enemyPortraitsSpriteIndex = ((IEnemyPortraitSprite)leaf).EnemyPortraitsSpriteIndex;
        sb.Append(enemyPortraitsSpriteIndex ?? -1);
        sb.Append(',');
        sb.Append(!leaf.CanBeSpied);
        sb.Append(',');
        sb.Append(leaf.EventIdOnFall ?? -1);
        sb.Append(',');
        sb.Append((int)leaf.HitActionTrigger);
        sb.Append(',');
        sb.Append(leaf.CanActWhileStunned);
        sb.Append(',');
        sb.Append(leaf.SizeWhenFrozen);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, EnemyLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        IDictionary<int, EnemyLeaf> enemiesByGameId = _enemiesRegistry.LeavesByGameIds;

        leaf.EntityAnimId = int.Parse(fields[0]);
        leaf.BaseMaxHp = int.Parse(fields[1]);
        leaf.BaseDefense = int.Parse(fields[2]);
        leaf.BaseExpReward = int.Parse(fields[3]);
        leaf.BaseBerriesDropAmount = int.Parse(fields[4]);
        leaf.CursorOffset = new(float.Parse(fields[5]), float.Parse(fields[6]), float.Parse(fields[7]));
        leaf.PoisonResistance = int.Parse(fields[8]);
        leaf.FreezeResistance = int.Parse(fields[9]);
        leaf.NumbResistance = int.Parse(fields[10]);
        leaf.SleepResistance = int.Parse(fields[11]);
        leaf.LogicalSize = float.Parse(fields[12]);
        leaf.EntityFreezeSize = new(float.Parse(fields[13]), float.Parse(fields[14]), float.Parse(fields[15]));
        leaf.EntityFreezeOffset = new(float.Parse(fields[16]), float.Parse(fields[17]), float.Parse(fields[18]));
        leaf.StartingBattlePosition = Enum.Parse<BattleControl.BattlePosition>(fields[19]);
        leaf.EntityHeight = float.Parse(fields[20]);
        leaf.EntityBobSpeed = float.Parse(fields[21]);
        leaf.EntityBobRange = float.Parse(fields[22]);

        leaf.Properties.Clear();
        string[] properties = fields[23].Split(
            StringUtils.OpeningBraceSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < properties.Length; i++)
            leaf.Properties.Add(Enum.Parse<BattleControl.AttackProperty>(properties[i]));

        leaf.Weight = float.Parse(fields[24]);
        int baseEnemyId = int.Parse(fields[25]);
        leaf.BaseEnemyId = baseEnemyId < 0 ? null : new(enemiesByGameId[baseEnemyId]);
        int eventIdOnDeath = int.Parse(fields[26]);
        leaf.EventIdOnDeath = eventIdOnDeath == 1 ? -1 : eventIdOnDeath;
        leaf.ActorTurnAmountPerMainTurn = int.Parse(fields[27]);
        leaf.CanBeTaunted = !bool.Parse(fields[28]);
        leaf.CanFall = !bool.Parse(fields[29]);
        leaf.HasFixedExpScaling = bool.Parse(fields[30]);
        leaf.IsAffectedByExhaustion = !bool.Parse(fields[31]);
        leaf.HasStatsHiddenFromHud = bool.Parse(fields[32]);
        leaf.InternalDeathType = int.Parse(fields[33]);

        leaf.EnemiesWhoTriggerHitActionWhenDamaged.Clear();
        if (!string.IsNullOrWhiteSpace(fields[34]) && fields[34] != "-1")
        {
            string[] enemyIds = fields[34].Split(StringUtils.SemiColonSplitDelimiter);
            foreach (string enemyId in enemyIds)
                leaf.EnemiesWhoTriggerHitActionWhenDamaged.Add(new(enemiesByGameId[int.Parse(enemyId)]));
        }

        leaf.HardModeAttackIncrease = int.Parse(fields[35]);
        leaf.HardModeBaseMaxHpIncrease = int.Parse(fields[36]);
        leaf.HardModeBaseDefenseIncrease = int.Parse(fields[37]);
        leaf.DefenseIncreaseWhenDefending = int.Parse(fields[38]);
        leaf.ItemOffset = new(float.Parse(fields[39]), float.Parse(fields[40]), float.Parse(fields[41]));
        leaf.IsBaseStateBattleIdle = bool.Parse(fields[42]);
        ((IEnemyPortraitSprite)leaf).EnemyPortraitsSpriteIndex = int.Parse(fields[43]);
        leaf.CanBeSpied = !bool.Parse(fields[44]);
        int eventIdOnFall = int.Parse(fields[45]);
        leaf.EventIdOnFall = eventIdOnFall == -1 ? null : eventIdOnFall;
        leaf.HitActionTrigger = (EnemyLeaf.AutoHitActionTrigger)int.Parse(fields[46]);
        leaf.CanActWhileStunned = bool.Parse(fields[47]);
        leaf.SizeWhenFrozen = float.Parse(fields[48]);
    }
}