using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;

internal sealed class MapEntityTextAssetParser : IMapEntityTextAssetParser
{
    private readonly ILogger<MapEntityTextAssetParser> _logger;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;
    private readonly IRegistryResolver _registryResolver;

    public MapEntityTextAssetParser(
        ILogger<MapEntityTextAssetParser> logger,
        ILeavesRegistry<FlagLeaf> flagsRegistry,
        IRegistryResolver registryResolver)
    {
        _logger = logger;
        _registryResolver = registryResolver;
        _flagsRegistry = flagsRegistry;
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
        sb.Append(value.InternalPrimaryBehavior.ToString());
        sb.Append('}');
        sb.Append(value.InternalSecondaryBehavior.ToString());
        sb.Append('}');
        sb.Append(value.InternalNpcInteraction.ToString());
        sb.Append('}');
        sb.Append(value.InternalDeathType.ToString());
        sb.Append('}');
        sb.Append(value.StartingPosition.x);
        sb.Append('}');
        sb.Append(value.StartingPosition.y);
        sb.Append('}');
        sb.Append(value.StartingPosition.z);
        sb.Append('}');
        sb.Append(value.InternalAnimIdOrItemId);
        sb.Append('}');
        sb.Append(value.InternalIsFlipped);
        sb.Append('}');
        sb.Append(value.InternalCcolHeight);
        sb.Append('}');
        sb.Append(value.InternalCcolRadius);
        sb.Append('}');
        sb.Append(value.InternalRadius);
        sb.Append('}');
        sb.Append(value.InternalTimer);
        sb.Append('}');
        sb.Append(value.InternalSpeed);
        sb.Append('}');
        sb.Append(value.InternalPrimaryActionFrequency);
        sb.Append('}');
        sb.Append(value.InternalSecondaryActionFrequency);
        sb.Append('}');
        sb.Append(value.InternalSpeedMultiplier);
        sb.Append('}');
        sb.Append(value.InternalRadiusLimit);
        sb.Append('}');
        sb.Append(value.InternalWanderRadius);
        sb.Append('}');
        sb.Append(value.InternalTeleportRadius);
        sb.Append('}');
        sb.Append(value.InternalHaxBoxCol);
        sb.Append('}');
        sb.Append(value.InternalBoxColIsTrigger);
        sb.Append('}');
        sb.Append(value.InternalBoxColSize.x);
        sb.Append('}');
        sb.Append(value.InternalBoxColSize.y);
        sb.Append('}');
        sb.Append(value.InternalBoxColSize.z);
        sb.Append('}');
        sb.Append(value.InternalBoxColCenter.x);
        sb.Append('}');
        sb.Append(value.InternalBoxColCenter.y);
        sb.Append('}');
        sb.Append(value.InternalBoxColCenter.z);
        sb.Append('}');
        sb.Append(value.InternalFreezeTime);
        sb.Append('}');
        sb.Append(value.InternalFreezeSize.x);
        sb.Append('}');
        sb.Append(value.InternalFreezeSize.y);
        sb.Append('}');
        sb.Append(value.InternalFreezeSize.z);
        sb.Append('}');
        sb.Append(value.InternalFreezeOffset.x);
        sb.Append('}');
        sb.Append(value.InternalFreezeOffset.y);
        sb.Append('}');
        sb.Append(value.InternalFreezeOffset.z);
        sb.Append('}');
        sb.Append(value.InternalEventId);
        sb.Append('}');

        sb.Append(value.Requires.Count);
        sb.Append('}');

        List<int> allRequires = GetListPaddedWithOriginalArray(
            value.Requires.Select(r => r.GameId).ToList(),
            value.OriginalRequires);
        foreach (int require in allRequires)
        {
            sb.Append(require);
            sb.Append('}');
        }

        sb.Append(value.Limit.Count);
        sb.Append('}');

        List<int> allLimitsValues = value.Limit
            .Select(l => l.FailsWholeConditionWhenFlagIsTrue
                ? -l.Flag.GameId
                : l.Flag.GameId)
            .ToList();
        List<int> allLimits = GetListPaddedWithOriginalArray(allLimitsValues, value.OriginalLimits);
        foreach (int limit in allLimits)
        {
            sb.Append(limit);
            sb.Append('}');
        }

        sb.Append(value.InternalData.Count);
        sb.Append('}');

        List<int> allData = GetListPaddedWithOriginalArray(value.InternalData, value.OriginalData);
        foreach (int data in allData)
        {
            sb.Append(data);
            sb.Append('}');
        }

        sb.Append(value.InternalVectorData.Count);
        sb.Append('}');

        List<Vector3> allVectorData =
            GetListPaddedWithOriginalArray(value.InternalVectorData, value.OriginalVectorData);
        foreach (Vector3 vectorData in allVectorData)
        {
            sb.Append(vectorData.x);
            sb.Append('}');
            sb.Append(vectorData.y);
            sb.Append('}');
            sb.Append(vectorData.z);
            sb.Append('}');
        }

        sb.Append(value.InternalDialogues.Count);
        sb.Append('}');

        List<Vector3> allDialogues = GetListPaddedWithOriginalArray(value.InternalDialogues, value.OriginalDialogues);
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

        sb.Append(value.InternalBattleEnemyIds.Count);
        sb.Append('}');

        List<int> allBattleEnemyIds =
            GetListPaddedWithOriginalArray(value.InternalBattleEnemyIds, value.OriginalBattleEnemyIds);
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
        sb.Append(value.InternalEmoticonOffset.x);
        sb.Append('}');
        sb.Append(value.InternalEmoticonOffset.y);
        sb.Append('}');
        sb.Append(value.InternalEmoticonOffset.z);
        sb.Append('}');
        sb.Append(value.InsideId);
        sb.Append('}');

        List<Vector2> allEmoticonFlags =
            GetListPaddedWithOriginalArray(value.InternalEmoticonFlags, value.OriginalEmoticonFlags);
        foreach (Vector2 emoticonFlag in allEmoticonFlags)
        {
            sb.Append(emoticonFlag.x);
            sb.Append(',');
            sb.Append(emoticonFlag.y);
            sb.Append('}');
        }

        sb.Append(value.InternalSpyDialogueMapId);
        sb.Append('}');
        sb.Append(value.InternalRegionalFlagId);
        sb.Append('}');
        sb.Append(value.InitialHeight);
        sb.Append('}');
        sb.Append(value.InternalBobRange);
        sb.Append('}');
        sb.Append(value.InternalBobSpeed);
        sb.Append('}');
        sb.Append(value.InternalActivationFlagId);
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

    public MapEntity FromTextAssetSerializedString(int id, string name, string text)
    {
        string[] fields = text.Split(StringUtils.ClosingBraceSplitDelimiter);

        NPCControl.NPCType type = Enum.Parse<NPCControl.NPCType>(fields[0]);
        NPCControl.ObjectTypes objectType = Enum.Parse<NPCControl.ObjectTypes>(fields[1]);
        MapEntity value = GetTypedMapEntity(type, objectType);

        value.Id = id;
        value.Name = name;
        value.OriginalType = type;
        value.OriginalObjectType = objectType;
        value.InternalPrimaryBehavior = Enum.Parse<NPCControl.ActionBehaviors>(fields[2]);
        value.InternalSecondaryBehavior = Enum.Parse<NPCControl.ActionBehaviors>(fields[3]);
        value.InternalNpcInteraction = Enum.Parse<NPCControl.Interaction>(fields[4]);
        value.InternalDeathType = Enum.Parse<NPCControl.DeathType>(fields[5]);
        value.StartingPosition = new(float.Parse(fields[6]), float.Parse(fields[7]), float.Parse(fields[8]));
        value.InternalAnimIdOrItemId = int.Parse(fields[9]);
        value.InternalIsFlipped = bool.Parse(fields[10]);
        value.InternalCcolHeight = float.Parse(fields[11]);
        value.InternalCcolRadius = float.Parse(fields[12]);
        value.InternalRadius = float.Parse(fields[13]);
        value.InternalTimer = float.Parse(fields[14]);
        value.InternalSpeed = float.Parse(fields[15]);
        value.InternalPrimaryActionFrequency = float.Parse(fields[16]);
        value.InternalSecondaryActionFrequency = float.Parse(fields[17]);
        value.InternalSpeedMultiplier = float.Parse(fields[18]);
        value.InternalRadiusLimit = float.Parse(fields[19]);
        value.InternalWanderRadius = float.Parse(fields[20]);
        value.InternalTeleportRadius = float.Parse(fields[21]);
        value.InternalHaxBoxCol = bool.Parse(fields[22]);
        value.InternalBoxColIsTrigger = bool.Parse(fields[23]);
        value.InternalBoxColSize = new(float.Parse(fields[24]), float.Parse(fields[25]), float.Parse(fields[26]));
        value.InternalBoxColCenter = new(float.Parse(fields[27]), float.Parse(fields[28]), float.Parse(fields[29]));
        value.InternalFreezeTime = float.Parse(fields[30]);
        value.InternalFreezeSize = new(float.Parse(fields[31]), float.Parse(fields[32]), float.Parse(fields[33]));
        value.InternalFreezeOffset = new(float.Parse(fields[34]), float.Parse(fields[35]), float.Parse(fields[36]));
        value.InternalEventId = int.Parse(fields[37]);

        int requiresLength = int.Parse(fields[38]);
        for (int i = 0; i < 10; i++)
            value.OriginalRequires[i] = int.Parse(fields[39 + i]);
        for (int i = 0; i < requiresLength; i++)
            value.Requires.Add(new(_flagsRegistry.LeavesByGameIds[value.OriginalRequires[i]]));

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
        {
            value.Limit.Add(
                new()
                {
                    Flag = new(_flagsRegistry.LeavesByGameIds[Math.Abs(value.OriginalLimits[i])]),
                    FailsWholeConditionWhenFlagIsTrue = value.OriginalLimits[i] < 0
                });
        }

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
            value.InternalData.Add(value.OriginalData[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.InternalData),
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
            value.InternalVectorData.Add(value.OriginalVectorData[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.InternalVectorData),
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
            value.InternalDialogues.Add(value.OriginalDialogues[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.InternalDialogues),
                dialoguesLength,
                value.OriginalDialogues);
        }

        value.EulerAngles = new(float.Parse(fields[163]), float.Parse(fields[164]), float.Parse(fields[165]));

        int battleEnemyIdsLength = int.Parse(fields[166]);
        for (int i = 0; i < 4; i++)
            value.OriginalBattleEnemyIds[i] = int.Parse(fields[167 + i]);
        for (int i = 0; i < battleEnemyIdsLength; i++)
            value.InternalBattleEnemyIds.Add(value.OriginalBattleEnemyIds[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.Name,
                nameof(MapEntity.InternalBattleEnemyIds),
                battleEnemyIdsLength,
                value.OriginalBattleEnemyIds);
        }

        value.TagColor = new(
            float.Parse(fields[171]),
            float.Parse(fields[172]),
            float.Parse(fields[173]),
            float.Parse(fields[174]));
        value.InternalEmoticonOffset = new(
            float.Parse(fields[175]),
            float.Parse(fields[176]),
            float.Parse(fields[177]));
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
                if (value.InternalEmoticonFlags.Count == 1)
                    value.InternalEmoticonFlags.Clear();
                continue;
            }

            value.InternalEmoticonFlags.Add(value.OriginalEmoticonFlags[i]);
        }

        value.InternalSpyDialogueMapId = int.Parse(fields[189]);
        value.InternalRegionalFlagId = int.Parse(fields[190]);
        value.InitialHeight = float.Parse(fields[191]);
        value.InternalBobRange = float.Parse(fields[192]);
        value.InternalBobSpeed = float.Parse(fields[193]);
        value.InternalActivationFlagId = int.Parse(fields[194]);

        value.IsReturnToHeightOriginallyInt = fields[195].Length == 1;
        if (value.IsReturnToHeightOriginallyInt)
            value.ReturnToHeight = int.Parse(fields[195]) != 0;
        else
            value.ReturnToHeight = bool.Parse(fields[195]);

        if (fields.Length > 196)
            value.UnusedOverflowData = string.Join("}", fields.Skip(196));

        value.InitializeFromExisting(_registryResolver);
        return value;
    }

    private MapEntity GetTypedMapEntity(NPCControl.NPCType type, NPCControl.ObjectTypes objectType) =>
        (type, objectType) switch
        {
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.BeetleGrass) => new BeetleGrassMapEntity(),
            _ => new BlankMapEntity()
        };

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