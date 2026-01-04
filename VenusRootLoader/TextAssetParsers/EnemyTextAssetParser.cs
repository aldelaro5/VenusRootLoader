using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

// TODO: Handle bestiary ordering (just has the game id, one per line)
internal sealed class EnemyTextAssetParser : ITextAssetSerializable<EnemyLeaf>
{
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
        sb.Append(leaf.BaseBerriesDrop);
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
        sb.Append(leaf.Size);
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
        sb.Append(leaf.BaseEnemyId);
        sb.Append(',');
        sb.Append(leaf.EventIdOnDeath);
        sb.Append(',');
        sb.Append(leaf.ActorTurnAmountPerMainTurn);
        sb.Append(',');
        sb.Append(leaf.CannotBeTaunted);
        sb.Append(',');
        sb.Append(leaf.CannotFall);
        sb.Append(',');
        sb.Append(leaf.HasFixedExpScaling);
        sb.Append(',');
        sb.Append(leaf.DoesNotHaveExhaustion);
        sb.Append(',');
        sb.Append(leaf.HasStatsHiddenFromHud);
        sb.Append(',');
        sb.Append(leaf.DeathMethod);
        sb.Append(',');
        if (leaf.EnemyIdsWhoTriggersHitActionOnHit.Count == 0)
            sb.Append("-1");
        else
            sb.Append(string.Join(";", leaf.EnemyIdsWhoTriggersHitActionOnHit.Select(id => id.ToString())));
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
        sb.Append(leaf.UseBattleIdleAsEntityBaseState);
        sb.Append(',');
        sb.Append(leaf.EnemyPortraitsSpriteIndex);
        sb.Append(',');
        sb.Append(leaf.CannotBeSpied);
        sb.Append(',');
        sb.Append(leaf.EventIdOnFall);
        sb.Append(',');
        sb.Append(leaf.HitActionTrigger);
        sb.Append(',');
        sb.Append(leaf.CanActWhileStunned);
        sb.Append(',');
        sb.Append(leaf.SizeOnFreeze);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, EnemyLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.EntityAnimId = int.Parse(fields[0]);
        leaf.BaseMaxHp = int.Parse(fields[1]);
        leaf.BaseDefense = int.Parse(fields[2]);
        leaf.BaseExpReward = int.Parse(fields[3]);
        leaf.BaseBerriesDrop = int.Parse(fields[4]);
        leaf.CursorOffset = new(float.Parse(fields[5]), float.Parse(fields[6]), float.Parse(fields[7]));
        leaf.PoisonResistance = int.Parse(fields[8]);
        leaf.FreezeResistance = int.Parse(fields[9]);
        leaf.NumbResistance = int.Parse(fields[10]);
        leaf.SleepResistance = int.Parse(fields[11]);
        leaf.Size = float.Parse(fields[12]);
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
        leaf.BaseEnemyId = int.Parse(fields[25]);
        leaf.EventIdOnDeath = int.Parse(fields[26]);
        leaf.ActorTurnAmountPerMainTurn = int.Parse(fields[27]);
        leaf.CannotBeTaunted = bool.Parse(fields[28]);
        leaf.CannotFall = bool.Parse(fields[29]);
        leaf.HasFixedExpScaling = bool.Parse(fields[30]);
        leaf.DoesNotHaveExhaustion = bool.Parse(fields[31]);
        leaf.HasStatsHiddenFromHud = bool.Parse(fields[32]);
        leaf.DeathMethod = int.Parse(fields[33]);

        leaf.EnemyIdsWhoTriggersHitActionOnHit.Clear();
        if (fields[34] != "-1")
        {
            string[] enemyIds = fields[34].Split(StringUtils.SemiColonSplitDelimiter);
            foreach (string enemyId in enemyIds)
                leaf.EnemyIdsWhoTriggersHitActionOnHit.Add(int.Parse(enemyId));
        }

        leaf.HardModeAttackIncrease = int.Parse(fields[35]);
        leaf.HardModeBaseMaxHpIncrease = int.Parse(fields[36]);
        leaf.HardModeBaseDefenseIncrease = int.Parse(fields[37]);
        leaf.DefenseIncreaseWhenDefending = int.Parse(fields[38]);
        leaf.ItemOffset = new(float.Parse(fields[39]), float.Parse(fields[40]), float.Parse(fields[41]));
        leaf.UseBattleIdleAsEntityBaseState = bool.Parse(fields[42]);
        leaf.EnemyPortraitsSpriteIndex = int.Parse(fields[43]);
        leaf.CannotBeSpied = bool.Parse(fields[44]);
        leaf.EventIdOnFall = int.Parse(fields[45]);
        leaf.HitActionTrigger = int.Parse(fields[46]);
        leaf.CanActWhileStunned = bool.Parse(fields[47]);
        leaf.SizeOnFreeze = float.Parse(fields[48]);
    }
}