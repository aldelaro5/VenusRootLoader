using System.Globalization;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class MapEntityTextAssetParser : ITextAssetParser<MapLeaf.MapEntity>
{
    public string GetTextAssetSerializedString(string subPath, MapLeaf.MapEntity value)
    {
        if (subPath.EndsWith("names", StringComparison.OrdinalIgnoreCase))
            return value.Name;
        
        StringBuilder sb = new();

        sb.Append(value.Type.ToString());
        sb.Append('}');
        sb.Append(value.ObjectType.ToString());
        sb.Append('}');
        sb.Append(value.PrimaryBehavior.ToString());
        sb.Append('}');
        sb.Append(value.SecondaryBehavior.ToString());
        sb.Append('}');
        sb.Append(value.NpcInteraction.ToString());
        sb.Append('}');
        sb.Append(value.DeathType.ToString());
        sb.Append('}');
        sb.Append(value.StartingPosition.x);
        sb.Append('}');
        sb.Append(value.StartingPosition.y);
        sb.Append('}');
        sb.Append(value.StartingPosition.z);
        sb.Append('}');
        sb.Append(value.AnimIdOrItemId);
        sb.Append('}');
        sb.Append(value.IsFlipped);
        sb.Append('}');
        sb.Append(value.CcolHeight);
        sb.Append('}');
        sb.Append(value.CcolRadius);
        sb.Append('}');
        sb.Append(value.Radius);
        sb.Append('}');
        sb.Append(value.Timer);
        sb.Append('}');
        sb.Append(value.Speed);
        sb.Append('}');
        sb.Append(value.PrimaryActionFrequency);
        sb.Append('}');
        sb.Append(value.SecondaryActionFrequency);
        sb.Append('}');
        sb.Append(value.SpeedMultiplier);
        sb.Append('}');
        sb.Append(value.RadiusLimit);
        sb.Append('}');
        sb.Append(value.WanderRadius);
        sb.Append('}');
        sb.Append(value.TeleportRadius);
        sb.Append('}');
        sb.Append(value.HaxBoxCol);
        sb.Append('}');
        sb.Append(value.BoxColIsTrigger);
        sb.Append('}');
        sb.Append(value.BoxColSize.x);
        sb.Append('}');
        sb.Append(value.BoxColSize.y);
        sb.Append('}');
        sb.Append(value.BoxColSize.z);
        sb.Append('}');
        sb.Append(value.BoxColCenter.x);
        sb.Append('}');
        sb.Append(value.BoxColCenter.y);
        sb.Append('}');
        sb.Append(value.BoxColCenter.z);
        sb.Append('}');
        sb.Append(value.FreezeTime);
        sb.Append('}');
        sb.Append(value.FreezeSize.x);
        sb.Append('}');
        sb.Append(value.FreezeSize.y);
        sb.Append('}');
        sb.Append(value.FreezeSize.z);
        sb.Append('}');
        sb.Append(value.FreezeOffset.x);
        sb.Append('}');
        sb.Append(value.FreezeOffset.y);
        sb.Append('}');
        sb.Append(value.FreezeOffset.z);
        sb.Append('}');
        sb.Append(value.EventId);
        sb.Append('}');

        sb.Append(value.RequiresLength);
        sb.Append('}');
        for (int i = 0; i < 10; i++)
        {
            sb.Append(value.Requires[i]);
            sb.Append('}');
        }

        sb.Append(value.LimitsLength);
        sb.Append('}');
        for (int i = 0; i < 10; i++)
        {
            sb.Append(value.Limit[i]);
            sb.Append('}');
        }

        sb.Append(value.DataLength);
        sb.Append('}');
        for (int i = 0; i < 10; i++)
        {
            sb.Append(value.Data[i]);
            sb.Append('}');
        }

        sb.Append(value.VectorDataLength);
        sb.Append('}');
        for (int i = 0; i < 10; i++)
        {
            sb.Append(value.VectorData[i].x);
            sb.Append('}');
            sb.Append(value.VectorData[i].y);
            sb.Append('}');
            sb.Append(value.VectorData[i].z);
            sb.Append('}');
        }

        sb.Append(value.DialoguesLength);
        sb.Append('}');
        for (int i = 0; i < 20; i++)
        {
            sb.Append(value.Dialogues[i].x);
            sb.Append('}');
            sb.Append(value.Dialogues[i].y);
            sb.Append('}');
            sb.Append(value.Dialogues[i].z);
            sb.Append('}');
        }

        sb.Append(value.EulerAngles.x);
        sb.Append('}');
        sb.Append(value.EulerAngles.y);
        sb.Append('}');
        sb.Append(value.EulerAngles.z);
        sb.Append('}');

        sb.Append(value.BattleEnemyIdsLength);
        sb.Append('}');
        for (int i = 0; i < 4; i++)
        {
            sb.Append(value.BattleEnemyIds[i]);
            sb.Append('}');
        }

        sb.Append(value.TagColor.r);
        sb.Append('}');
        sb.Append(value.TagColor.g);
        sb.Append('}');
        sb.Append(value.TagColor.b);
        sb.Append('}');
        sb.Append(value.TagColor.a);
        sb.Append('}');
        sb.Append(value.EmoticonOffset.x);
        sb.Append('}');
        sb.Append(value.EmoticonOffset.y);
        sb.Append('}');
        sb.Append(value.EmoticonOffset.z);
        sb.Append('}');
        sb.Append(value.InsideId);
        sb.Append('}');

        for (int i = 0; i < 10; i++)
        {
            sb.Append(value.EmoticonFlags[i].x);
            sb.Append(',');
            sb.Append(value.EmoticonFlags[i].y);
            sb.Append('}');
        }

        sb.Append(value.SpyDialogueMapId);
        sb.Append('}');
        sb.Append(value.RegionalFlagId);
        sb.Append('}');
        sb.Append(value.InitialHeight);
        sb.Append('}');
        sb.Append(value.BobRange);
        sb.Append('}');
        sb.Append(value.BobSpeed);
        sb.Append('}');
        sb.Append(value.ActivationFlagId);
        sb.Append('}');

        string valueReturnToHeightString;
        if (value.IsReturnToHeightOriginallyInt)
        {
            int returnToHeightInt = value.ReturnToHeight ? 1 : 0;
            valueReturnToHeightString = returnToHeightInt.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            valueReturnToHeightString = value.ReturnToHeight.ToString();
        }

        sb.Append(valueReturnToHeightString);

        if (!string.IsNullOrEmpty(value.UnusedOverflowData))
        {
            sb.Append('}');
            sb.Append(value.UnusedOverflowData);
        }
        
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, MapLeaf.MapEntity value)
    {
        if (subPath.EndsWith("names", StringComparison.OrdinalIgnoreCase))
        {
            value.Name = text;
            return;
        }

        string[] fields = text.Split(StringUtils.ClosingBraceSplitDelimiter);

        value.Type = Enum.Parse<NPCControl.NPCType>(fields[0]);
        value.ObjectType = Enum.Parse<NPCControl.ObjectTypes>(fields[1]);
        value.PrimaryBehavior = Enum.Parse<NPCControl.ActionBehaviors>(fields[2]);
        value.SecondaryBehavior = Enum.Parse<NPCControl.ActionBehaviors>(fields[3]);
        value.NpcInteraction = Enum.Parse<NPCControl.Interaction>(fields[4]);
        value.DeathType = Enum.Parse<NPCControl.DeathType>(fields[5]);
        value.StartingPosition = new(float.Parse(fields[6]), float.Parse(fields[7]), float.Parse(fields[8]));
        value.AnimIdOrItemId = int.Parse(fields[9]);
        value.IsFlipped = bool.Parse(fields[10]);
        value.CcolHeight = float.Parse(fields[11]);
        value.CcolRadius = float.Parse(fields[12]);
        value.Radius = float.Parse(fields[13]);
        value.Timer = float.Parse(fields[14]);
        value.Speed = float.Parse(fields[15]);
        value.PrimaryActionFrequency = float.Parse(fields[16]);
        value.SecondaryActionFrequency = float.Parse(fields[17]);
        value.SpeedMultiplier = float.Parse(fields[18]);
        value.RadiusLimit = float.Parse(fields[19]);
        value.WanderRadius = float.Parse(fields[20]);
        value.TeleportRadius = float.Parse(fields[21]);
        value.HaxBoxCol = bool.Parse(fields[22]);
        value.BoxColIsTrigger = bool.Parse(fields[23]);
        value.BoxColSize = new(float.Parse(fields[24]), float.Parse(fields[25]), float.Parse(fields[26]));
        value.BoxColCenter = new(float.Parse(fields[27]), float.Parse(fields[28]), float.Parse(fields[29]));
        value.FreezeTime = float.Parse(fields[30]);
        value.FreezeSize = new(float.Parse(fields[31]), float.Parse(fields[32]), float.Parse(fields[33]));
        value.FreezeOffset = new(float.Parse(fields[34]), float.Parse(fields[35]), float.Parse(fields[36]));
        value.EventId = int.Parse(fields[37]);

        value.RequiresLength = int.Parse(fields[38]);
        for (int i = 0; i < 10; i++)
            value.Requires[i] = int.Parse(fields[39 + i]);

        value.LimitsLength = int.Parse(fields[49]);
        for (int i = 0; i < 10; i++)
            value.Limit[i] = int.Parse(fields[50 + i]);

        value.DataLength = int.Parse(fields[60]);
        for (int i = 0; i < 10; i++)
            value.Data[i] = int.Parse(fields[61 + i]);

        value.VectorDataLength = int.Parse(fields[71]);
        for (int i = 0; i < 10; i++)
        {
            value.VectorData[i] = new(
                float.Parse(fields[72 + (i * 3)]),
                float.Parse(fields[72 + (i * 3) + 1]),
                float.Parse(fields[72 + (i * 3) + 2]));
        }

        value.DialoguesLength = int.Parse(fields[102]);
        for (int i = 0; i < 20; i++)
        {
            value.Dialogues[i] = new(
                    float.Parse(fields[103 + (i * 3)]),
                    float.Parse(fields[103 + (i * 3) + 1]),
                    float.Parse(fields[103 + (i * 3) + 2]));
        }

        value.EulerAngles = new(float.Parse(fields[163]), float.Parse(fields[164]), float.Parse(fields[165]));

        value.BattleEnemyIdsLength = int.Parse(fields[166]);
        for (int i = 0; i < 4; i++)
            value.BattleEnemyIds[i] = int.Parse(fields[167 + i]);

        value.TagColor = new(
            float.Parse(fields[171]),
            float.Parse(fields[172]),
            float.Parse(fields[173]),
            float.Parse(fields[174]));
        value.EmoticonOffset = new(float.Parse(fields[175]), float.Parse(fields[176]), float.Parse(fields[177]));
        value.InsideId = int.Parse(fields[178]);

        for (int i = 0; i < 10; i++)
        {
            string[] components = fields[179 + i].Split(StringUtils.CommaSplitDelimiter);
            value.EmoticonFlags[i] = new(float.Parse(components[0]), float.Parse(components[1]));
        }

        value.SpyDialogueMapId = int.Parse(fields[189]);
        value.RegionalFlagId = int.Parse(fields[190]);
        value.InitialHeight = float.Parse(fields[191]);
        value.BobRange = float.Parse(fields[192]);
        value.BobSpeed = float.Parse(fields[193]);
        value.ActivationFlagId = int.Parse(fields[194]);

        value.IsReturnToHeightOriginallyInt = fields[195].Length == 1;
        if (value.IsReturnToHeightOriginallyInt)
            value.ReturnToHeight = int.Parse(fields[195]) != 0;
        else
            value.ReturnToHeight = bool.Parse(fields[195]);

        if (fields.Length > 196)
            value.UnusedOverflowData = string.Join("}", fields.Skip(196));
    }
}