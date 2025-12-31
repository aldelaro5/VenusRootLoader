using System.Text;
using UnityEngine;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.Enemies;

public sealed class EnemyData : ITextAssetSerializable
{
    public int EntityAnimId { get; set; }
    public int MaxHp { get; set; }
    public int BaseDefense { get; set; }
    public int BaseExpDrop { get; set; }
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
    public int BaseEnemyId { get; set; } = -1;
    public int EventIdOnDeath { get; set; } = -1;
    public int ActorTurnsAmountPerMainTurn { get; set; }
    public bool CannotBeTaunted { get; set; }
    public bool CannotFall { get; set; }
    public bool HasFixedExpDropScaling { get; set; }
    public bool HasNoExhaustion { get; set; }
    public bool HasHpAndDefenseHiddenInHud { get; set; }
    public int DeathMethod { get; set; }
    public List<int> HitActionOnHitEnemyIds { get; } = new();
    public int HardModeAttackIncrease { get; set; }
    public int HardModeMaxHpBaseIncrease { get; set; }
    public int HardModeDefenseBaseIncrease { get; set; }
    public int DefenseIncreaseWhenDefending { get; set; }
    public Vector3 ItemOffset { get; set; } = Vector3.zero;
    public bool HasBattleIdleBaseAnimState { get; set; }
    internal int EnemyPortraitSpriteIndex { get; set; } = -1;
    public bool CannotBeSpied { get; set; }
    public int EventIdOnFall { get; set; } = -1;
    public int HitActionHitTrigger { get; set; }
    public bool CanActWhileStunned { get; set; }
    public float SizeOnFreeze { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(EntityAnimId);
        sb.Append(',');
        sb.Append(MaxHp);
        sb.Append(',');
        sb.Append(BaseDefense);
        sb.Append(',');
        sb.Append(BaseExpDrop);
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
        sb.Append(ActorTurnsAmountPerMainTurn);
        sb.Append(',');
        sb.Append(CannotBeTaunted);
        sb.Append(',');
        sb.Append(CannotFall);
        sb.Append(',');
        sb.Append(HasFixedExpDropScaling);
        sb.Append(',');
        sb.Append(HasNoExhaustion);
        sb.Append(',');
        sb.Append(HasHpAndDefenseHiddenInHud);
        sb.Append(',');
        sb.Append(DeathMethod);
        sb.Append(',');
        sb.Append(
            HitActionOnHitEnemyIds.Count != 0
                ? string.Join(";", HitActionOnHitEnemyIds.Select(x => x.ToString()))
                : "-1");
        sb.Append(',');
        sb.Append(HardModeAttackIncrease);
        sb.Append(',');
        sb.Append(HardModeMaxHpBaseIncrease);
        sb.Append(',');
        sb.Append(HardModeDefenseBaseIncrease);
        sb.Append(',');
        sb.Append(DefenseIncreaseWhenDefending);
        sb.Append(',');
        sb.Append(ItemOffset.x);
        sb.Append(',');
        sb.Append(ItemOffset.y);
        sb.Append(',');
        sb.Append(ItemOffset.z);
        sb.Append(',');
        sb.Append(HasBattleIdleBaseAnimState);
        sb.Append(',');
        sb.Append(EnemyPortraitSpriteIndex);
        sb.Append(',');
        sb.Append(CannotBeSpied);
        sb.Append(',');
        sb.Append(EventIdOnFall);
        sb.Append(',');
        sb.Append(HitActionHitTrigger);
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
        MaxHp = int.Parse(fields[1]);
        BaseDefense = int.Parse(fields[2]);
        BaseExpDrop = int.Parse(fields[3]);
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
        // Skip 0 because it is the length which is redundant
        for (int i = 1; i < properties.Length; i++)
            Properties.Add(Enum.Parse<BattleControl.AttackProperty>(properties[i]));

        Weight = float.Parse(fields[24]);
        BaseEnemyId = int.Parse(fields[25]);
        EventIdOnDeath = int.Parse(fields[26]);
        ActorTurnsAmountPerMainTurn = int.Parse(fields[27]);
        CannotBeTaunted = bool.Parse(fields[28]);
        CannotFall = bool.Parse(fields[29]);
        HasFixedExpDropScaling = bool.Parse(fields[30]);
        HasNoExhaustion = bool.Parse(fields[31]);
        HasHpAndDefenseHiddenInHud = bool.Parse(fields[32]);
        DeathMethod = int.Parse(fields[33]);

        HitActionOnHitEnemyIds.Clear();
        if (fields[34] != "-1")
            HitActionOnHitEnemyIds.AddRange(fields[34].Split(StringUtils.SemiColonSplitDelimiter).Select(int.Parse));

        HardModeAttackIncrease = int.Parse(fields[35]);
        HardModeMaxHpBaseIncrease = int.Parse(fields[36]);
        HardModeDefenseBaseIncrease = int.Parse(fields[37]);
        DefenseIncreaseWhenDefending = int.Parse(fields[38]);
        ItemOffset = new(float.Parse(fields[39]), float.Parse(fields[40]), float.Parse(fields[41]));
        HasBattleIdleBaseAnimState = bool.Parse(fields[42]);
        EnemyPortraitSpriteIndex = int.Parse(fields[43]);
        CannotBeSpied = bool.Parse(fields[44]);
        EventIdOnFall = int.Parse(fields[45]);
        HitActionHitTrigger = int.Parse(fields[46]);
        CanActWhileStunned = bool.Parse(fields[47]);
        SizeOnFreeze = float.Parse(fields[48]);
    }
}