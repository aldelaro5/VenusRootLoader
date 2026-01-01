using System.Text;
using UnityEngine;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Entities;

public sealed class MapEntity : ITextAssetSerializable
{
    public NPCControl.NPCType Type { get; set; }
    public NPCControl.ObjectTypes ObjectType { get; set; }
    public NPCControl.ActionBehaviors PrimaryBehavior { get; set; }
    public NPCControl.ActionBehaviors SecondaryBehavior { get; set; }
    public NPCControl.Interaction NpcInteraction { get; set; }
    public NPCControl.DeathType DeathType { get; set; }
    public Vector3 StartingPosition { get; set; }
    public int AnimIdOrItemId { get; set; }
    public bool IsFlipped { get; set; }
    public float CcolHeight { get; set; }
    public float CcolRadius { get; set; }
    public float Radius { get; set; }
    public float Timer { get; set; }
    public float Speed { get; set; }
    public float PrimaryActionFrequency { get; set; }
    public float SecondaryActionFrequency { get; set; }
    public float SpeedMultiplier { get; set; }
    public float RadiusLimit { get; set; }
    public float WanderRadius { get; set; }
    public float TeleportRadius { get; set; }
    public bool HaxBoxCol { get; set; }
    public bool BoxColIsTrigger { get; set; }
    public Vector3 BoxColSize { get; set; } = Vector3.one;
    public Vector3 BoxColCenter { get; set; }
    public float FreezeTime { get; set; }
    public Vector3 FreezeSize { get; set; } = Vector3.one;
    public Vector3 FreezeOffset { get; set; } = Vector3.zero;
    public int EventId { get; set; }
    public List<int> Requires { get; } = new();
    public List<int> Limit { get; } = new();
    public List<int> Data { get; } = new();
    public List<Vector3> VectorData { get; } = new();
    public List<Vector3> Dialogues { get; } = new();
    public Vector3 EulerAngles { get; set; }
    public List<int> BattleEnemyIds { get; } = new();
    public Color TagColor { get; set; }
    public Vector3 EmoticonOffset { get; set; } = Vector3.zero;
    public int InsideId { get; set; }
    public List<Vector2> EmoticonFlags { get; } = new();
    public int SpyDialogueMapId { get; set; }
    public int RegionalFlagId { get; set; }
    public float InitialHeight { get; set; }
    public float BobRange { get; set; }
    public float BobSpeed { get; set; }
    public int ActivationFlagId { get; set; }
    public bool ReturnToHeight { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();

        sb.Append(Type.ToString());
        sb.Append('}');
        sb.Append(ObjectType.ToString());
        sb.Append('}');
        sb.Append(PrimaryBehavior.ToString());
        sb.Append('}');
        sb.Append(SecondaryBehavior.ToString());
        sb.Append('}');
        sb.Append(NpcInteraction.ToString());
        sb.Append('}');
        sb.Append(DeathType.ToString());
        sb.Append('}');
        sb.Append(StartingPosition.x);
        sb.Append('}');
        sb.Append(StartingPosition.y);
        sb.Append('}');
        sb.Append(StartingPosition.z);
        sb.Append('}');
        sb.Append(AnimIdOrItemId);
        sb.Append('}');
        sb.Append(IsFlipped);
        sb.Append('}');
        sb.Append(CcolHeight);
        sb.Append('}');
        sb.Append(CcolRadius);
        sb.Append('}');
        sb.Append(Radius);
        sb.Append('}');
        sb.Append(Timer);
        sb.Append('}');
        sb.Append(Speed);
        sb.Append('}');
        sb.Append(PrimaryActionFrequency);
        sb.Append('}');
        sb.Append(SecondaryActionFrequency);
        sb.Append('}');
        sb.Append(SpeedMultiplier);
        sb.Append('}');
        sb.Append(RadiusLimit);
        sb.Append('}');
        sb.Append(WanderRadius);
        sb.Append('}');
        sb.Append(TeleportRadius);
        sb.Append('}');
        sb.Append(HaxBoxCol);
        sb.Append('}');
        sb.Append(BoxColIsTrigger);
        sb.Append('}');
        sb.Append(BoxColSize.x);
        sb.Append('}');
        sb.Append(BoxColSize.y);
        sb.Append('}');
        sb.Append(BoxColSize.z);
        sb.Append('}');
        sb.Append(BoxColCenter.x);
        sb.Append('}');
        sb.Append(BoxColCenter.y);
        sb.Append('}');
        sb.Append(BoxColCenter.z);
        sb.Append('}');
        sb.Append(FreezeTime);
        sb.Append('}');
        sb.Append(FreezeSize.x);
        sb.Append('}');
        sb.Append(FreezeSize.y);
        sb.Append('}');
        sb.Append(FreezeSize.z);
        sb.Append('}');
        sb.Append(FreezeOffset.x);
        sb.Append('}');
        sb.Append(FreezeOffset.y);
        sb.Append('}');
        sb.Append(FreezeOffset.z);
        sb.Append('}');
        sb.Append(EventId);
        sb.Append('}');

        sb.Append(Requires.Count);
        sb.Append('}');
        for (int i = 0; i < 10; i++)
        {
            if (i >= Requires.Count)
            {
                sb.Append("0}");
                continue;
            }

            int require = Requires[i];
            sb.Append(require);
            sb.Append('}');
        }

        sb.Append(Limit.Count);
        sb.Append('}');
        for (int i = 0; i < 10; i++)
        {
            if (i >= Limit.Count)
            {
                sb.Append("0}");
                continue;
            }

            int limit = Limit[i];
            sb.Append(limit);
            sb.Append('}');
        }

        sb.Append(Data.Count);
        sb.Append('}');
        for (int i = 0; i < 10; i++)
        {
            if (i >= Data.Count)
            {
                sb.Append("0}");
                continue;
            }

            int data = Data[i];
            sb.Append(data);
            sb.Append('}');
        }

        sb.Append(VectorData.Count);
        sb.Append('}');
        for (int i = 0; i < 10; i++)
        {
            if (i >= VectorData.Count)
            {
                sb.Append("0}0}0}");
                continue;
            }

            Vector3 vector = VectorData[i];
            sb.Append(vector.x);
            sb.Append('}');
            sb.Append(vector.y);
            sb.Append('}');
            sb.Append(vector.z);
            sb.Append('}');
        }

        sb.Append(Dialogues.Count);
        sb.Append('}');
        for (int i = 0; i < 20; i++)
        {
            if (i >= Dialogues.Count)
            {
                sb.Append("0}0}0}");
                continue;
            }

            Vector3 dialogue = Dialogues[i];
            sb.Append(dialogue.x);
            sb.Append('}');
            sb.Append(dialogue.y);
            sb.Append('}');
            sb.Append(dialogue.z);
            sb.Append('}');
        }

        sb.Append(EulerAngles.x);
        sb.Append('}');
        sb.Append(EulerAngles.y);
        sb.Append('}');
        sb.Append(EulerAngles.z);
        sb.Append('}');

        sb.Append(BattleEnemyIds.Count);
        sb.Append('}');
        for (int i = 0; i < 4; i++)
        {
            if (i >= BattleEnemyIds.Count)
            {
                sb.Append("0}");
                continue;
            }

            int battleEnemyId = BattleEnemyIds[i];
            sb.Append(battleEnemyId);
            sb.Append('}');
        }

        sb.Append(TagColor.r);
        sb.Append('}');
        sb.Append(TagColor.g);
        sb.Append('}');
        sb.Append(TagColor.b);
        sb.Append('}');
        sb.Append(TagColor.a);
        sb.Append('}');
        sb.Append(EmoticonOffset.x);
        sb.Append('}');
        sb.Append(EmoticonOffset.y);
        sb.Append('}');
        sb.Append(EmoticonOffset.z);
        sb.Append('}');
        sb.Append(InsideId);
        sb.Append('}');

        for (int i = 0; i < 10; i++)
        {
            if (i >= EmoticonFlags.Count)
            {
                sb.Append("0}0}");
                continue;
            }

            Vector2 emoticonFlag = EmoticonFlags[i];
            sb.Append(emoticonFlag.x);
            sb.Append('}');
            sb.Append(emoticonFlag.y);
            sb.Append('}');
        }

        sb.Append(SpyDialogueMapId);
        sb.Append('}');
        sb.Append(RegionalFlagId);
        sb.Append('}');
        sb.Append(InitialHeight);
        sb.Append('}');
        sb.Append(BobRange);
        sb.Append('}');
        sb.Append(BobSpeed);
        sb.Append('}');
        sb.Append(ActivationFlagId);
        sb.Append('}');
        sb.Append(ReturnToHeight);

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.ClosingBraceSplitDelimiter);

        Type = Enum.Parse<NPCControl.NPCType>(fields[0]);
        ObjectType = Enum.Parse<NPCControl.ObjectTypes>(fields[1]);
        PrimaryBehavior = Enum.Parse<NPCControl.ActionBehaviors>(fields[2]);
        SecondaryBehavior = Enum.Parse<NPCControl.ActionBehaviors>(fields[3]);
        NpcInteraction = Enum.Parse<NPCControl.Interaction>(fields[4]);
        DeathType = Enum.Parse<NPCControl.DeathType>(fields[5]);
        StartingPosition = new(float.Parse(fields[6]), float.Parse(fields[7]), float.Parse(fields[8]));
        AnimIdOrItemId = int.Parse(fields[9]);
        IsFlipped = bool.Parse(fields[10]);
        CcolHeight = float.Parse(fields[11]);
        CcolRadius = float.Parse(fields[12]);
        Radius = float.Parse(fields[13]);
        Timer = float.Parse(fields[14]);
        Speed = float.Parse(fields[15]);
        PrimaryActionFrequency = float.Parse(fields[16]);
        SecondaryActionFrequency = float.Parse(fields[17]);
        SpeedMultiplier = float.Parse(fields[18]);
        RadiusLimit = float.Parse(fields[19]);
        WanderRadius = float.Parse(fields[20]);
        TeleportRadius = float.Parse(fields[21]);
        HaxBoxCol = bool.Parse(fields[22]);
        BoxColIsTrigger = bool.Parse(fields[23]);
        BoxColSize = new(float.Parse(fields[24]), float.Parse(fields[25]), float.Parse(fields[26]));
        BoxColCenter = new(float.Parse(fields[27]), float.Parse(fields[28]), float.Parse(fields[29]));
        FreezeTime = float.Parse(fields[30]);
        FreezeSize = new(float.Parse(fields[31]), float.Parse(fields[32]), float.Parse(fields[33]));
        FreezeOffset = new(float.Parse(fields[34]), float.Parse(fields[35]), float.Parse(fields[36]));
        EventId = int.Parse(fields[37]);

        Requires.Clear();
        for (int i = 0; i < int.Parse(fields[38]); i++)
            Requires.Add(int.Parse(fields[39 + i]));

        Limit.Clear();
        for (int i = 0; i < int.Parse(fields[49]); i++)
            Limit.Add(int.Parse(fields[50 + i]));

        Data.Clear();
        for (int i = 0; i < int.Parse(fields[60]); i++)
            Data.Add(int.Parse(fields[61 + i]));

        VectorData.Clear();
        for (int i = 0; i < int.Parse(fields[71]); i++)
        {
            VectorData.Add(
                new(
                    float.Parse(fields[72 + (i * 3)]),
                    float.Parse(fields[72 + (i * 3) + 1]),
                    float.Parse(fields[72 + (i * 3) + 2])));
        }

        VectorData.Clear();
        for (int i = 0; i < int.Parse(fields[102]); i++)
        {
            Dialogues.Add(
                new(
                    float.Parse(fields[103 + (i * 3)]),
                    float.Parse(fields[103 + (i * 3) + 1]),
                    float.Parse(fields[103 + (i * 3) + 2])));
        }

        EulerAngles = new(float.Parse(fields[163]), float.Parse(fields[164]), float.Parse(fields[165]));

        BattleEnemyIds.Clear();
        for (int i = 0; i < int.Parse(fields[166]); i++)
            BattleEnemyIds.Add(int.Parse(fields[167 + i]));

        TagColor = new(
            float.Parse(fields[171]),
            float.Parse(fields[172]),
            float.Parse(fields[173]),
            float.Parse(fields[174]));
        EmoticonOffset = new(float.Parse(fields[175]), float.Parse(fields[176]), float.Parse(fields[177]));
        InsideId = int.Parse(fields[178]);

        EmoticonFlags.Clear();
        for (int i = 0; i < 10; i++)
        {
            string[] components = fields[179 + (i * 2)].Split(StringUtils.CommaSplitDelimiter);
            EmoticonFlags.Add(new(float.Parse(components[0]), float.Parse(components[1])));
        }

        SpyDialogueMapId = int.Parse(fields[189]);
        RegionalFlagId = int.Parse(fields[190]);
        InitialHeight = float.Parse(fields[191]);
        BobRange = float.Parse(fields[192]);
        BobSpeed = float.Parse(fields[193]);
        ActivationFlagId = int.Parse(fields[194]);

        if (fields[195].Length > 1)
            ReturnToHeight = int.Parse(fields[195]) != 0;
        else
            ReturnToHeight = bool.Parse(fields[195]);
    }
}