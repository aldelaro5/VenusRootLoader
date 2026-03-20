using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.MapEntities;
using VenusRootLoader.Extensions;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class MapEntityTextAssetParser : ITextAssetParser<MapEntity>
{
    private readonly ILogger<MapEntityTextAssetParser> _logger;

    public MapEntityTextAssetParser(ILogger<MapEntityTextAssetParser> logger)
    {
        _logger = logger;
    }

    public string GetTextAssetSerializedString(string subPath, MapEntity value)
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

        sb.Append(value.Requires.Count);
        sb.Append('}');

        List<int> allRequires = GetListPaddedWithOriginalArray(value.Requires, value.OriginalRequires);
        foreach (int require in allRequires)
        {
            sb.Append(require);
            sb.Append('}');
        }

        sb.Append(value.Limit.Count);
        sb.Append('}');

        List<int> allLimits = GetListPaddedWithOriginalArray(value.Limit, value.OriginalLimits);
        foreach (int limit in allLimits)
        {
            sb.Append(limit);
            sb.Append('}');
        }

        sb.Append(value.Data.Count);
        sb.Append('}');

        List<int> allData = GetListPaddedWithOriginalArray(value.Data, value.OriginalData);
        foreach (int data in allData)
        {
            sb.Append(data);
            sb.Append('}');
        }

        sb.Append(value.VectorData.Count);
        sb.Append('}');

        List<Vector3> allVectorData = GetListPaddedWithOriginalArray(value.VectorData, value.OriginalVectorData);
        foreach (Vector3 vectorData in allVectorData)
        {
            sb.Append(vectorData.x);
            sb.Append('}');
            sb.Append(vectorData.y);
            sb.Append('}');
            sb.Append(vectorData.z);
            sb.Append('}');
        }

        sb.Append(value.Dialogues.Count);
        sb.Append('}');

        List<Vector3> allDialogues = GetListPaddedWithOriginalArray(value.Dialogues, value.OriginalDialogues);
        foreach (Vector3 dialogue in allDialogues)
        {
            sb.Append(dialogue.x);
            sb.Append('}');
            sb.Append(dialogue.y);
            sb.Append('}');
            sb.Append(dialogue.z);
            sb.Append('}');
        }

        sb.Append(value.EulerAngles.x);
        sb.Append('}');
        sb.Append(value.EulerAngles.y);
        sb.Append('}');
        sb.Append(value.EulerAngles.z);
        sb.Append('}');

        sb.Append(value.BattleEnemyIds.Count);
        sb.Append('}');

        List<int> allBattleEnemyIds =
            GetListPaddedWithOriginalArray(value.BattleEnemyIds, value.OriginalBattleEnemyIds);
        foreach (int battleEnemyId in allBattleEnemyIds)
        {
            sb.Append(battleEnemyId);
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

        List<Vector2> allEmoticonFlags =
            GetListPaddedWithOriginalArray(value.EmoticonFlags, value.OriginalEmoticonFlags);
        foreach (Vector2 emoticonFlag in allEmoticonFlags)
        {
            sb.Append(emoticonFlag.x);
            sb.Append(',');
            sb.Append(emoticonFlag.y);
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

    public void FromTextAssetSerializedString(string subPath, string text, MapEntity value)
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

        int requiresLength = int.Parse(fields[38]);
        for (int i = 0; i < 10; i++)
            value.OriginalRequires[i] = int.Parse(fields[39 + i]);
        for (int i = 0; i < requiresLength; i++)
            value.Requires.Add(value.OriginalRequires[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.Requires),
                requiresLength,
                value.OriginalRequires);
        }

        int limitsLength = int.Parse(fields[49]);
        for (int i = 0; i < 10; i++)
            value.OriginalLimits[i] = int.Parse(fields[50 + i]);
        for (int i = 0; i < limitsLength; i++)
            value.Limit.Add(value.OriginalLimits[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.Limit),
                limitsLength,
                value.OriginalLimits);
        }

        int dataLength = int.Parse(fields[60]);
        for (int i = 0; i < 10; i++)
            value.OriginalData[i] = int.Parse(fields[61 + i]);
        for (int i = 0; i < dataLength; i++)
            value.Data.Add(value.OriginalData[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.Data),
                dataLength,
                value.OriginalData);
        }

        int vectorDataLength = int.Parse(fields[71]);
        for (int i = 0; i < 10; i++)
        {
            value.OriginalVectorData[i] = new(
                float.Parse(fields[72 + (i * 3)]),
                float.Parse(fields[72 + (i * 3) + 1]),
                float.Parse(fields[72 + (i * 3) + 2]));
        }

        for (int i = 0; i < vectorDataLength; i++)
            value.VectorData.Add(value.OriginalVectorData[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.VectorData),
                vectorDataLength,
                value.OriginalVectorData);
        }

        int dialoguesLength = int.Parse(fields[102]);
        for (int i = 0; i < 20; i++)
        {
            value.OriginalDialogues[i] = new(
                float.Parse(fields[103 + (i * 3)]),
                float.Parse(fields[103 + (i * 3) + 1]),
                float.Parse(fields[103 + (i * 3) + 2]));
        }

        for (int i = 0; i < dialoguesLength; i++)
            value.Dialogues.Add(value.OriginalDialogues[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.Dialogues),
                dialoguesLength,
                value.OriginalDialogues);
        }

        value.EulerAngles = new(float.Parse(fields[163]), float.Parse(fields[164]), float.Parse(fields[165]));

        int battleEnemyIdsLength = int.Parse(fields[166]);
        for (int i = 0; i < 4; i++)
            value.OriginalBattleEnemyIds[i] = int.Parse(fields[167 + i]);
        for (int i = 0; i < battleEnemyIdsLength; i++)
            value.BattleEnemyIds.Add(value.OriginalBattleEnemyIds[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.BattleEnemyIds),
                battleEnemyIdsLength,
                value.OriginalBattleEnemyIds);
        }

        value.TagColor = new(
            float.Parse(fields[171]),
            float.Parse(fields[172]),
            float.Parse(fields[173]),
            float.Parse(fields[174]));
        value.EmoticonOffset = new(float.Parse(fields[175]), float.Parse(fields[176]), float.Parse(fields[177]));
        value.InsideId = int.Parse(fields[178]);

        bool continueAddingIntoList = true;
        for (int i = 0; i < 10; i++)
        {
            string[] components = fields[179 + i].Split(StringUtils.CommaSplitDelimiter);
            value.OriginalEmoticonFlags[i] = new(float.Parse(components[0]), float.Parse(components[1]));

            if (!continueAddingIntoList)
                continue;
            if (i > 0 && (int)value.OriginalEmoticonFlags[i].x < 0)
            {
                continueAddingIntoList = false;
                if (value.EmoticonFlags.Count == 1)
                    value.EmoticonFlags.Clear();
                continue;
            }

            value.EmoticonFlags.Add(value.OriginalEmoticonFlags[i]);
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

    private void LogIfListHasUnreadableData(string entityName, string listName, int expectedLength, int[] array)
    {
        if (array.All(x => x == 0))
            return;

        int lastNonZeroIndex = array.Select((x, i) => (x, i)).Last(x => x.x != 0).i;
        if (lastNonZeroIndex > expectedLength - 1)
        {
            _logger.LogTrace(
                "{EntityName}.{listName} has unreadable non 0 data: declared length: {length}, full data: {data}",
                entityName,
                listName,
                expectedLength,
                string.Join(" | ", array));
        }
    }

    private void LogIfListHasUnreadableData(string entityName, string listName, int expectedLength, Vector3[] array)
    {
        if (array.All(x => x == default))
            return;

        int lastNonDefaultIndex = array.Select((x, i) => (x, i)).Last(x => x.x != default).i;
        if (lastNonDefaultIndex > expectedLength - 1)
        {
            _logger.LogTrace(
                "{EntityName}.{listName} has unreadable non default data: declared length: {length}, full data: {data}",
                entityName,
                listName,
                expectedLength,
                string.Join(" | ", array));
        }
    }

    private static List<T> GetListPaddedWithOriginalArray<T>(List<T> newList, T[] originalArray)
    {
        return newList.Concat(
                originalArray
                    .Skip(newList.Count)
                    .Take(originalArray.Length - newList.Count))
            .ToList();
    }
}