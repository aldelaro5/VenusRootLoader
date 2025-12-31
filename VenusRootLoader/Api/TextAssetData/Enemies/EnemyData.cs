using System.Text;
using UnityEngine;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Enemies;

public sealed class EnemyData : ITextAssetSerializable
{
    public int EntityAnimId { get; set; }
    public int BaseMaxHp { get; set; }
    public int BaseDefense { get; set; }
    public int BaseExpReward { get; set; }
    public int BaseBerriesDrop { get; set; }
    public Vector3 CursorOffset { get; set; } = Vector3.zero;
    public int PoisonResistance { get; set; }
    public int FreezeResistance { get; set; }
    public int NumbResistance { get; set; }
    public int SleepResistance { get; set; }
    public float Size { get; set; }
    public Vector3 EntityFreezeSize { get; set; } = Vector3.one;
    public Vector3 EntityFreezeOffset { get; set; } = Vector3.zero;
    public BattleControl.BattlePosition StartingBattlePosition { get; set; }
    public float EntityHeight { get; set; }
    public float EntityBobSpeed { get; set; }
    public float EntityBobRange { get; set; }
    public List<BattleControl.AttackProperty> Properties { get; } = new();
    public float Weight { get; set; }
    public int BaseEnemyId { get; set; }
    public int EventIdOnDeath { get; set; }
    public int ActorTurnAmountPerMainTurn { get; set; }
    public bool CannotBeTaunted { get; set; }
    public bool CannotFall { get; set; }
    public bool HasFixedExpScaling { get; set; }
    public bool DoesNotHaveExhaustion { get; set; }
    public bool HasStatsHiddenFromHud { get; set; }
    public int DeathMethod { get; set; }
    public List<int> EnemyIdsWhoTriggersHitActionOnHit { get; } = new();
    public int HardModeAttackIncrease { get; set; }
    public int HardModeBaseMaxHpIncrease { get; set; }
    public int HardModeBaseDefenseIncrease { get; set; }
    public int DefenseIncreaseWhenDefending { get; set; }
    public Vector3 ItemOffset { get; set; } = Vector3.zero;
    public bool UseBattleIdleAsEntityBaseState { get; set; }
    internal int EnemyPortraitsSpriteIndex { get; set; }
    public bool CannotBeSpied { get; set; }
    public int EventIdOnFall { get; set; }
    public int HitActionTrigger { get; set; }
    public bool CanActWhileStunned { get; set; }
    public float SizeOnFreeze { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();

        sb.Append(EntityAnimId);
        sb.Append(',');
        sb.Append(BaseMaxHp);
        sb.Append(',');
        sb.Append(BaseDefense);
        sb.Append(',');
        sb.Append(BaseExpReward);
        sb.Append(',');
        sb.Append(BaseBerriesDrop);
        sb.Append(',');
        sb.Append(CursorOffset.x);
        sb.Append(',');
        sb.Append(CursorOffset.y);
        sb.Append(',');
        sb.Append(CursorOffset.z);
        sb.Append(',');
        sb.Append(PoisonResistance);
        sb.Append(',');
        sb.Append(FreezeResistance);
        sb.Append(',');
        sb.Append(NumbResistance);
        sb.Append(',');
        sb.Append(SleepResistance);
        sb.Append(',');
        sb.Append(Size);
        sb.Append(',');
        sb.Append(EntityFreezeSize.x);
        sb.Append(',');
        sb.Append(EntityFreezeSize.y);
        sb.Append(',');
        sb.Append(EntityFreezeSize.z);
        sb.Append(',');
        sb.Append(EntityFreezeOffset.x);
        sb.Append(',');
        sb.Append(EntityFreezeOffset.y);
        sb.Append(',');
        sb.Append(EntityFreezeOffset.z);
        sb.Append(',');
        sb.Append(StartingBattlePosition.ToString());
        sb.Append(',');
        sb.Append(EntityHeight);
        sb.Append(',');
        sb.Append(EntityBobSpeed);
        sb.Append(',');
        sb.Append(EntityBobRange);
        sb.Append(',');

        sb.Append(Properties.Count);
        foreach (BattleControl.AttackProperty property in Properties)
        {
            sb.Append('{');
            sb.Append(property.ToString());
        }

        sb.Append("{,");

        sb.Append(Weight);
        sb.Append(',');
        sb.Append(BaseEnemyId);
        sb.Append(',');
        sb.Append(EventIdOnDeath);
        sb.Append(',');
        sb.Append(ActorTurnAmountPerMainTurn);
        sb.Append(',');
        sb.Append(CannotBeTaunted);
        sb.Append(',');
        sb.Append(CannotFall);
        sb.Append(',');
        sb.Append(HasFixedExpScaling);
        sb.Append(',');
        sb.Append(DoesNotHaveExhaustion);
        sb.Append(',');
        sb.Append(HasStatsHiddenFromHud);
        sb.Append(',');
        sb.Append(DeathMethod);
        sb.Append(',');
        if (EnemyIdsWhoTriggersHitActionOnHit.Count == 0)
            sb.Append("-1");
        else
            sb.Append(string.Join(";", EnemyIdsWhoTriggersHitActionOnHit.Select(id => id.ToString())));
        sb.Append(',');
        sb.Append(HardModeAttackIncrease);
        sb.Append(',');
        sb.Append(HardModeBaseMaxHpIncrease);
        sb.Append(',');
        sb.Append(HardModeBaseDefenseIncrease);
        sb.Append(',');
        sb.Append(DefenseIncreaseWhenDefending);
        sb.Append(',');
        sb.Append(ItemOffset.x);
        sb.Append(',');
        sb.Append(ItemOffset.y);
        sb.Append(',');
        sb.Append(ItemOffset.z);
        sb.Append(',');
        sb.Append(UseBattleIdleAsEntityBaseState);
        sb.Append(',');
        sb.Append(EnemyPortraitsSpriteIndex);
        sb.Append(',');
        sb.Append(CannotBeSpied);
        sb.Append(',');
        sb.Append(EventIdOnFall);
        sb.Append(',');
        sb.Append(HitActionTrigger);
        sb.Append(',');
        sb.Append(CanActWhileStunned);
        sb.Append(',');
        sb.Append(SizeOnFreeze);

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        EntityAnimId = int.Parse(fields[0]);
        BaseMaxHp = int.Parse(fields[1]);
        BaseDefense = int.Parse(fields[2]);
        BaseExpReward = int.Parse(fields[3]);
        BaseBerriesDrop = int.Parse(fields[4]);
        CursorOffset = new(float.Parse(fields[5]), float.Parse(fields[6]), float.Parse(fields[7]));
        PoisonResistance = int.Parse(fields[8]);
        FreezeResistance = int.Parse(fields[9]);
        NumbResistance = int.Parse(fields[10]);
        SleepResistance = int.Parse(fields[11]);
        Size = float.Parse(fields[12]);
        EntityFreezeSize = new(float.Parse(fields[13]), float.Parse(fields[14]), float.Parse(fields[15]));
        EntityFreezeOffset = new(float.Parse(fields[16]), float.Parse(fields[17]), float.Parse(fields[18]));
        StartingBattlePosition = Enum.Parse<BattleControl.BattlePosition>(fields[19]);
        EntityHeight = float.Parse(fields[20]);
        EntityBobSpeed = float.Parse(fields[21]);
        EntityBobRange = float.Parse(fields[22]);

        Properties.Clear();
        string[] properties = fields[23].Split(
            StringUtils.OpeningBraceSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < properties.Length; i++)
            Properties.Add(Enum.Parse<BattleControl.AttackProperty>(properties[i]));

        Weight = float.Parse(fields[24]);
        BaseEnemyId = int.Parse(fields[25]);
        EventIdOnDeath = int.Parse(fields[26]);
        ActorTurnAmountPerMainTurn = int.Parse(fields[27]);
        CannotBeTaunted = bool.Parse(fields[28]);
        CannotFall = bool.Parse(fields[29]);
        HasFixedExpScaling = bool.Parse(fields[30]);
        DoesNotHaveExhaustion = bool.Parse(fields[31]);
        HasStatsHiddenFromHud = bool.Parse(fields[32]);
        DeathMethod = int.Parse(fields[33]);

        EnemyIdsWhoTriggersHitActionOnHit.Clear();
        if (fields[34] != "-1")
        {
            string[] enemyIds = fields[34].Split(StringUtils.SemiColonSplitDelimiter);
            foreach (string enemyId in enemyIds)
                EnemyIdsWhoTriggersHitActionOnHit.Add(int.Parse(enemyId));
        }

        HardModeAttackIncrease = int.Parse(fields[35]);
        HardModeBaseMaxHpIncrease = int.Parse(fields[36]);
        HardModeBaseDefenseIncrease = int.Parse(fields[37]);
        DefenseIncreaseWhenDefending = int.Parse(fields[38]);
        ItemOffset = new(float.Parse(fields[39]), float.Parse(fields[40]), float.Parse(fields[41]));
        UseBattleIdleAsEntityBaseState = bool.Parse(fields[42]);
        EnemyPortraitsSpriteIndex = int.Parse(fields[43]);
        CannotBeSpied = bool.Parse(fields[44]);
        EventIdOnFall = int.Parse(fields[45]);
        HitActionTrigger = int.Parse(fields[46]);
        CanActWhileStunned = bool.Parse(fields[47]);
        SizeOnFreeze = float.Parse(fields[48]);
    }
}