using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Api.Leaves.MapEntities.Enemies;
using VenusRootLoader.Api.Leaves.MapEntities.Npcs;
using VenusRootLoader.Api.Leaves.MapEntities.Objects;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;

/// <inheritdoc/>
internal sealed class MapEntityTextAssetParser : IMapEntityTextAssetParser
{
    private readonly ILogger<MapEntityTextAssetParser> _logger;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;

    private static readonly NPCControl.ActionBehaviors[] BehaviorsWithSecondaryVectorData =
    [
        NPCControl.ActionBehaviors.SetPath,
        NPCControl.ActionBehaviors.SetPathJump,
        NPCControl.ActionBehaviors.StealthAI
    ];

    public MapEntityTextAssetParser(
        ILogger<MapEntityTextAssetParser> logger,
        ILeavesRegistry<FlagLeaf> flagsRegistry)
    {
        _logger = logger;
        _flagsRegistry = flagsRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, MapEntityLeaf mapEntityLeaf)
    {
        if (subPath.EndsWith("names", StringComparison.OrdinalIgnoreCase))
            return mapEntityLeaf.BaseGameObjectName;

        StringBuilder sb = new();

        sb.Append(mapEntityLeaf.Type.ToString());
        sb.Append('}');
        sb.Append(mapEntityLeaf.ObjectType.ToString());
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalOutOfRangeBehavior.ToString());
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalInRangeBehavior.ToString());
        sb.Append('}');
        sb.Append(mapEntityLeaf.Interaction.ToString());
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalDeathType.ToString());
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalStartingPosition.x);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalStartingPosition.y);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalStartingPosition.z);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalAnimIdOrItemId);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalIsFlipped);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalCcolHeight);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalCcolRadius);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalRadius);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalTimer);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalSpeed);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalOutOfRangeActionFrequency);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalInRangeActionFrequency);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalSpeedMultiplier);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalRadiusLimit);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalWanderRadius);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalTeleportRadius);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalHaxBoxCol);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalBoxColIsTrigger);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalBoxColSize.x);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalBoxColSize.y);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalBoxColSize.z);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalBoxColCenter.x);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalBoxColCenter.y);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalBoxColCenter.z);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalFreezeTime);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalFreezeSize.x);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalFreezeSize.y);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalFreezeSize.z);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalFreezeOffset.x);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalFreezeOffset.y);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalFreezeOffset.z);
        sb.Append('}');
        sb.Append(
            mapEntityLeaf is { Type: NPCControl.NPCType.NPC, Interaction: NPCControl.Interaction.LockedDoor }
                ? 0
                : mapEntityLeaf.InternalEventId);
        sb.Append('}');

        sb.Append(mapEntityLeaf.InternalRequires.Count);
        sb.Append('}');

        List<int> allRequires = GetListPaddedWithOriginalArray(
            mapEntityLeaf.InternalRequires.Select(r => r.GameId).ToList(),
            mapEntityLeaf.OriginalRequires);
        foreach (int require in allRequires)
        {
            sb.Append(require);
            sb.Append('}');
        }

        sb.Append(mapEntityLeaf.InternalLimits.Count);
        sb.Append('}');

        List<int> allLimitsValues = mapEntityLeaf.InternalLimits
            .Select(l => l.FailsWholeConditionWhenFlagIsTrue
                ? -l.Flag.GameId
                : l.Flag.GameId)
            .ToList();
        List<int> allLimits = GetListPaddedWithOriginalArray(allLimitsValues, mapEntityLeaf.OriginalLimits);
        foreach (int limit in allLimits)
        {
            sb.Append(limit);
            sb.Append('}');
        }

        sb.Append(mapEntityLeaf.InternalData.Count);
        sb.Append('}');

        List<int> allData = GetListPaddedWithOriginalArray(mapEntityLeaf.InternalData, mapEntityLeaf.OriginalData);
        foreach (int data in allData)
        {
            sb.Append(data);
            sb.Append('}');
        }

        sb.Append(mapEntityLeaf.InternalVectorData.Count);
        sb.Append('}');

        List<Vector3> allVectorData =
            GetListPaddedWithOriginalArray(mapEntityLeaf.InternalVectorData, mapEntityLeaf.OriginalVectorData);
        foreach (Vector3 vectorData in allVectorData)
        {
            sb.Append(vectorData.x);
            sb.Append('}');
            sb.Append(vectorData.y);
            sb.Append('}');
            sb.Append(vectorData.z);
            sb.Append('}');
        }

        mapEntityLeaf.InternalSecondaryVectorDataArray = mapEntityLeaf.InternalSecondaryVectorData.ToArray();

        sb.Append(
            mapEntityLeaf is { Type: NPCControl.NPCType.NPC, Interaction: NPCControl.Interaction.Shop }
                ? 1
                : mapEntityLeaf.InternalDialogues.Count);
        sb.Append('}');

        List<Vector3> allDialogues = GetListPaddedWithOriginalArray(
            mapEntityLeaf.InternalDialogues,
            mapEntityLeaf.OriginalDialogues);
        foreach (Vector3 dialogue in allDialogues)
        {
            sb.Append(dialogue.x);
            sb.Append('}');
            sb.Append(dialogue.y);
            sb.Append('}');
            sb.Append(dialogue.z);
            sb.Append('}');
        }

        sb.Append(mapEntityLeaf.InternalEulerAngles.x);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalEulerAngles.y);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalEulerAngles.z);
        sb.Append('}');

        sb.Append(mapEntityLeaf.InternalBattleEnemyIds.Count);
        sb.Append('}');

        List<int> allBattleEnemyIds =
            GetListPaddedWithOriginalArray(mapEntityLeaf.InternalBattleEnemyIds, mapEntityLeaf.OriginalBattleEnemyIds);
        foreach (int battleEnemyId in allBattleEnemyIds)
        {
            sb.Append(battleEnemyId);
            sb.Append('}');
        }

        sb.Append(mapEntityLeaf.TagColor.r);
        sb.Append('}');
        sb.Append(mapEntityLeaf.TagColor.g);
        sb.Append('}');
        sb.Append(mapEntityLeaf.TagColor.b);
        sb.Append('}');
        sb.Append(mapEntityLeaf.TagColor.a);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalEmoticonOffset.x);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalEmoticonOffset.y);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalEmoticonOffset.z);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InsideId);
        sb.Append('}');

        List<Vector2> allEmoticonFlags =
            GetListPaddedWithOriginalArray(mapEntityLeaf.InternalEmoticonFlags, mapEntityLeaf.OriginalEmoticonFlags);
        foreach (Vector2 emoticonFlag in allEmoticonFlags)
        {
            sb.Append(emoticonFlag.x);
            sb.Append(',');
            sb.Append(emoticonFlag.y);
            sb.Append('}');
        }

        sb.Append(mapEntityLeaf.InternalSpyDialogueId);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalRegionalFlagId);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalInitialHeight);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalBobRange);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalBobSpeed);
        sb.Append('}');
        sb.Append(mapEntityLeaf.InternalActivationFlagId);
        sb.Append('}');

        string valueReturnToHeightString;
        if (mapEntityLeaf.IsReturnToHeightOriginallyInt)
        {
            int returnToHeightInt = mapEntityLeaf.InternalReturnToHeight ? 1 : 0;
            valueReturnToHeightString = returnToHeightInt.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            valueReturnToHeightString = mapEntityLeaf.InternalReturnToHeight.ToString();
        }

        sb.Append(valueReturnToHeightString);

        if (!string.IsNullOrEmpty(mapEntityLeaf.UnusedOverflowData))
        {
            sb.Append('}');
            sb.Append(mapEntityLeaf.UnusedOverflowData);
        }

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(MapLeaf map, string baseGameId, int id, string name, string text)
    {
        string[] fields = text.Split(StringUtils.ClosingBraceSplitDelimiter);

        // Map entities are unique in the sense there's dozens kinds of them that act completely differently from each
        // other are under, and they are under the same type and data format: NPCControl. This is very inconvenient and
        // to fix this, we have to create derived classes for each map entity type whose concrete type is resolved using
        // the NPCType and ObjectTypes fields. From there, each derived type can expose its own tailored API referencing
        // the base fields so we preserve parity on the base game side, but buds gets to see a more convenient representation.
        NPCControl.NPCType type = Enum.Parse<NPCControl.NPCType>(fields[0]);
        NPCControl.ObjectTypes objectType = Enum.Parse<NPCControl.ObjectTypes>(fields[1]);
        NPCControl.ActionBehaviors primaryBehavior = Enum.Parse<NPCControl.ActionBehaviors>(fields[2]);
        NPCControl.ActionBehaviors secondaryBehavior = Enum.Parse<NPCControl.ActionBehaviors>(fields[3]);
        NPCControl.Interaction interaction = Enum.Parse<NPCControl.Interaction>(fields[4]);
        MapEntityLeaf value = GetTypedMapEntity(
            map,
            id,
            baseGameId,
            type,
            objectType,
            interaction,
            primaryBehavior,
            secondaryBehavior,
            fields);

        value.BaseGameObjectName = name;
        value.Map = map;
        value.OriginalType = type;
        value.OriginalObjectType = objectType;
        value.OriginalInteraction = interaction;
        value.InternalOutOfRangeBehavior = primaryBehavior;
        value.InternalInRangeBehavior = secondaryBehavior;
        value.InternalDeathType = Enum.Parse<NPCControl.DeathType>(fields[5]);
        value.InternalStartingPosition = new(float.Parse(fields[6]), float.Parse(fields[7]), float.Parse(fields[8]));
        value.InternalAnimIdOrItemId = int.Parse(fields[9]);
        value.InternalIsFlipped = bool.Parse(fields[10]);
        value.InternalCcolHeight = float.Parse(fields[11]);
        value.InternalCcolRadius = float.Parse(fields[12]);
        value.InternalRadius = float.Parse(fields[13]);
        value.InternalTimer = float.Parse(fields[14]);
        value.InternalSpeed = float.Parse(fields[15]);
        value.InternalOutOfRangeActionFrequency = float.Parse(fields[16]);
        value.InternalInRangeActionFrequency = float.Parse(fields[17]);
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
            value.InternalRequires.Add(new(_flagsRegistry.LeavesByGameIds[value.OriginalRequires[i]]));

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.BaseGameObjectName,
                nameof(MapEntityLeaf.InternalRequires),
                requiresLength,
                value.OriginalRequires);
        }

        int limitsLength = int.Parse(fields[49]);
        for (int i = 0; i < 10; i++)
            value.OriginalLimits[i] = int.Parse(fields[50 + i]);
        for (int i = 0; i < limitsLength; i++)
        {
            value.InternalLimits.Add(
                new()
                {
                    Flag = new(_flagsRegistry.LeavesByGameIds[Math.Abs(value.OriginalLimits[i])]),
                    FailsWholeConditionWhenFlagIsTrue = value.OriginalLimits[i] < 0
                });
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.BaseGameObjectName,
                nameof(MapEntityLeaf.InternalLimits),
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
                value.BaseGameObjectName,
                nameof(MapEntityLeaf.InternalData),
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
        value.InternalSecondaryVectorData.AddRange(value.InternalVectorData);

        if (type is NPCControl.NPCType.NPC or NPCControl.NPCType.Enemy &&
            (BehaviorsWithSecondaryVectorData.Contains(primaryBehavior) ||
             BehaviorsWithSecondaryVectorData.Contains(secondaryBehavior)))
        {
            value.InternalVectorData.Clear();
        }

        if (value is not EnemyEncounterHoldingKeyItemMapEntityLeaf &&
            (BehaviorsWithSecondaryVectorData.Contains(primaryBehavior) ||
             BehaviorsWithSecondaryVectorData.Contains(secondaryBehavior)))
        {
            value.InternalVectorData.Clear();
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.BaseGameObjectName,
                nameof(MapEntityLeaf.InternalVectorData),
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

        if (interaction == NPCControl.Interaction.Shop)
        {
            value.InternalDialogues.AddRange(value.OriginalDialogues);
        }
        else
        {
            for (int i = 0; i < dialoguesLength; i++)
                value.InternalDialogues.Add(value.OriginalDialogues[i]);
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.BaseGameObjectName,
                nameof(MapEntityLeaf.InternalDialogues),
                dialoguesLength,
                value.OriginalDialogues);
        }

        value.InternalEulerAngles = new(float.Parse(fields[163]), float.Parse(fields[164]), float.Parse(fields[165]));

        int battleEnemyIdsLength = int.Parse(fields[166]);
        for (int i = 0; i < 4; i++)
            value.OriginalBattleEnemyIds[i] = int.Parse(fields[167 + i]);
        for (int i = 0; i < battleEnemyIdsLength; i++)
            value.InternalBattleEnemyIds.Add(value.OriginalBattleEnemyIds[i]);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.BaseGameObjectName,
                nameof(MapEntityLeaf.InternalBattleEnemyIds),
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

        for (int i = 0; i < 10; i++)
        {
            string[] components = fields[179 + i].Split(StringUtils.CommaSplitDelimiter);
            value.OriginalEmoticonFlags[i] = new(float.Parse(components[0]), float.Parse(components[1]));

            if (i == 0 || value.OriginalEmoticonFlags[i].x > 0)
                value.InternalEmoticonFlags.Add(value.OriginalEmoticonFlags[i]);
        }

        value.InternalSpyDialogueId = int.Parse(fields[189]);
        value.InternalRegionalFlagId = int.Parse(fields[190]);
        value.InternalInitialHeight = float.Parse(fields[191]);
        value.InternalBobRange = float.Parse(fields[192]);
        value.InternalBobSpeed = float.Parse(fields[193]);
        value.InternalActivationFlagId = int.Parse(fields[194]);

        value.IsReturnToHeightOriginallyInt = fields[195].Length == 1;
        if (value.IsReturnToHeightOriginallyInt)
            value.InternalReturnToHeight = int.Parse(fields[195]) != 0;
        else
            value.InternalReturnToHeight = bool.Parse(fields[195]);

        if (fields.Length > 196)
            value.UnusedOverflowData = string.Join("}", fields.Skip(196));
    }

    private static MapEntityLeaf GetTypedMapEntity(
        MapLeaf map,
        int id,
        string baseGameId,
        NPCControl.NPCType type,
        NPCControl.ObjectTypes objectType,
        NPCControl.Interaction interaction,
        NPCControl.ActionBehaviors primaryBehavior,
        NPCControl.ActionBehaviors secondaryBehavior,
        string[] fields)
    {
        ILeavesRegistry<MapEntityLeaf> registry = map.EntitiesRegistry;
        string namedId = id.ToString();

        return (type, objectType, interaction) switch
        {
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.BeetleGrass, _) =>
                registry.RegisterExisting<CuttableGrassMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.PushRock, _) =>
                int.Parse(fields[60]) < 3 ||
                int.Parse(fields[61 + 2]) == 0
                    ? registry.RegisterExisting<MovableRockMapEntityLeaf>(id, namedId, baseGameId)
                    : registry.RegisterExisting<SlidingIcePillarMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.PressurePlate, _) =>
                registry.RegisterExisting<PressurePlateMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.ANDGate, _) =>
                int.Parse(fields[60]) == 2 && int.Parse(fields[61 + 1]) == -1
                    ? registry.RegisterExisting<AndGateOnSingleFlagMapEntityLeaf>(id, namedId, baseGameId)
                    : int.Parse(fields[61 + 0]) switch
                    {
                        -2 => registry.RegisterExisting<AndGateOnFlagsMapEntityLeaf>(id, namedId, baseGameId),
                        >= -1 => registry.RegisterExisting<AndGateOnEntitiesLeafActivationMapEntityLeaf>(
                            id,
                            namedId,
                            baseGameId),
                        _ => ThrowHelper.ThrowArgumentOutOfRangeException<MapEntityLeaf>()
                    },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.CameraChange, _) => registry.RegisterExisting<
                CameraChangeMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.Item, _) => int.Parse(fields[61 + 0]) switch
            {
                0 or 1 => registry.RegisterExisting<CollectibleItemMapEntityLeaf>(id, namedId, baseGameId),
                2 => registry.RegisterExisting<CollectibleMedalMapEntityLeaf>(id, namedId, baseGameId),
                3 => registry.RegisterExisting<CollectibleCrystalBerryMapEntityLeaf>(id, namedId, baseGameId),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<MapEntityLeaf>()
            },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.DoorOtherMap, _) =>
                registry.RegisterExisting<LoadingZoneMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.SetPlayerRespawn, _) =>
                registry.RegisterExisting<SetPlayerRespawnZoneMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.DoorSameMap, _) =>
                registry.RegisterExisting<InsideTransitionZoneMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.EventTrigger, _) =>
                int.Parse(fields[60]) >= 3 &&
                int.Parse(fields[61 + 2]) == 1
                    ? registry.RegisterExisting<AutomaticEventTriggerMapEntityLeaf>(id, namedId, baseGameId)
                    : registry.RegisterExisting<EventTriggerZoneMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.DialogueTrigger, _) =>
                int.Parse(fields[60]) >= 3 &&
                int.Parse(fields[61 + 2]) == 1
                    ? registry.RegisterExisting<AutomaticMapDialogueTriggerMapEntityLeaf>(id, namedId, baseGameId)
                    : registry.RegisterExisting<DialogueTriggerZoneMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.ANDBlock, _) =>
                int.Parse(fields[60]) == 2 && int.Parse(fields[61 + 1]) == -1
                    ? registry.RegisterExisting<AndBlockOnSingleFlagMapEntityLeaf>(id, namedId, baseGameId)
                    : int.Parse(fields[61 + 0]) switch
                    {
                        -2 => registry.RegisterExisting<AndBlockOnFlagsMapEntityLeaf>(id, namedId, baseGameId),
                        >= -1 => registry.RegisterExisting<AndBlockOnEntitiesLeafActivationMapEntityLeaf>(
                            id,
                            namedId,
                            baseGameId),
                        _ => ThrowHelper.ThrowArgumentOutOfRangeException<MapEntityLeaf>()
                    },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.SavePoint, _) =>
                int.Parse(fields[61 + 1]) >= 10
                    ? registry.RegisterExisting<DeadLanderOmegaAlertCrystalMapEntityLeaf>(id, namedId, baseGameId)
                    : registry.RegisterExisting<SavePointCrystalMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.JumpSpring, _) =>
                int.Parse(fields[61 + 0]) == 1
                    ? registry.RegisterExisting<JumpToPositionSpringMapEntityLeaf>(id, namedId, baseGameId)
                    : registry.RegisterExisting<JumpUpSpringMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.DigSpot, _) =>
                (int.Parse(fields[61 + 0]), int.Parse(fields[61 + 1])) switch
                {
                    (1, _) => registry.RegisterExisting<DigSpotCrystalBerryMapEntityLeaf>(id, namedId, baseGameId),
                    (>= 2, _) => registry.RegisterExisting<DigSpotStartEventMapEntityLeaf>(id, namedId, baseGameId),
                    (<= 0, >= 2) => registry.RegisterExisting<DigSpotMedalMapEntityLeaf>(id, namedId, baseGameId),
                    (<= 0, < 2) => registry.RegisterExisting<DigSpotItemMapEntityLeaf>(id, namedId, baseGameId),
                },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.Switch, _) =>
                (int.Parse(fields[61 + 0]), int.Parse(fields[61 + 1]), int.Parse(fields[61 + 2])) switch
                {
                    (0, 0, 0) => registry.RegisterExisting<LatchedSwitchMapEntityLeaf>(id, namedId, baseGameId),
                    (0, 0, _) => registry.RegisterExisting<TimerSwitchMapEntityLeaf>(id, namedId, baseGameId),
                    (0, 1, _) => registry.RegisterExisting<LinkableToggleSwitchMapEntityLeaf>(id, namedId, baseGameId),
                    (1, >= 0, _) => registry.RegisterExisting<EventTriggerSwitchMapEntityLeaf>(id, namedId, baseGameId),
                    _ => ThrowHelper.ThrowArgumentOutOfRangeException<MapEntityLeaf>()
                },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.CoiledObject, _) =>
                registry.RegisterExisting<TrappedEntityLeafMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.FixedAnim, _) =>
                registry.RegisterExisting<FixedAnimstateMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.EnemySpawner, _) =>
                registry.RegisterExisting<EnemySpawnerMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.Dropplet, _) =>
                registry.RegisterExisting<FreezableWaterDropletMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.PathPlatform, _) =>
                (int)float.Parse(fields[103 + (1 * 3) + 0]) == 1
                    ? registry.RegisterExisting<MovingPlatformAlongLerpMapEntityLeaf>(id, namedId, baseGameId)
                    : registry.RegisterExisting<MovingPlatformAlongPathMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.BreakableRock, _) =>
                registry.RegisterExisting<BreakableRockMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.RotatingPlatform, _) =>
                registry.RegisterExisting<RotatingPlatformMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.Geizer, _) =>
                registry.RegisterExisting<GeyserMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.MusicRange, _) =>
                registry.RegisterExisting<MusicChangeZoneMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.TempPlatform, _) =>
                registry.RegisterExisting<FlytrapPlatformMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.ScrewSwitch, _) =>
                registry.RegisterExisting<SpinningCrankMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.ResetCamera, _) =>
                registry.RegisterExisting<ResetCameraZoneMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.StencilSwitch, _) =>
                registry.RegisterExisting<IceRadiusSwitchMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.RollingRock, _) => int.Parse(fields[61 + 2]) switch
            {
                1 => registry.RegisterExisting<RollingRockCanonMapEntityLeaf>(id, namedId, baseGameId),
                _ => registry.RegisterExisting<RollingRockMapEntityLeaf>(id, namedId, baseGameId)
            },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.TriggerSwitch, _) =>
                registry.RegisterExisting<SwitchTriggerZoneMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.WindPusher, _) =>
                registry.RegisterExisting<WindBeamZoneMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.WaterSwitch, _) =>
                registry.RegisterExisting<MapChildVerticalPositionSwitchMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.Enemy, _, _) => (int)float.Parse(fields[72 + (0 * 3) + 1]) == -2 &&
                                                !BehaviorsWithSecondaryVectorData.Contains(primaryBehavior) &&
                                                !BehaviorsWithSecondaryVectorData.Contains(secondaryBehavior)
                ? registry.RegisterExisting<EnemyEncounterHoldingKeyItemMapEntityLeaf>(id, namedId, baseGameId)
                : registry.RegisterExisting<EnemyEncounterWithRegularItemDropsMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.None) =>
                registry.RegisterExisting<NoInteractionNpcMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.Talk or NPCControl.Interaction.Check) =>
                registry.RegisterExisting<TalkingNpcMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.Event or NPCControl.Interaction.LockedDoor) =>
                registry.RegisterExisting<EventNpcMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.Shop) => float.Parse(fields[103 + (10 * 3)] + 0) == 0f
                ? registry.RegisterExisting<ItemsShopMapEntityLeaf>(id, namedId, baseGameId)
                : registry.RegisterExisting<MedalsShopMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.QuestBoard) =>
                registry.RegisterExisting<QuestBoardNpcMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.StorageAnt) =>
                registry.RegisterExisting<ItemsStorageNpcMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.CaravanBadge) =>
                registry.RegisterExisting<CaravanShelvedMedalNpcMapEntityLeaf>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.VenusHeal) =>
                registry.RegisterExisting<VenusHealingNpcMapEntityLeaf>(id, namedId, baseGameId),
            _ => ThrowHelper.ThrowInvalidOperationException<MapEntityLeaf>(
                $"Invalid NPCControl - type: {type}, ObjectType: {objectType}, Interaction: {interaction}")
        };
    }

    // This allows to basically preserve as much as possible the original array from base game, but only if the new list
    // wouldn't exceed the length of the base game one. This wouldn't impact logic because ultimately, the game only cares
    // about the advertised length in the data which will be ours to control. It's just to copy leftover, possibly unused data.
    private static List<T> GetListPaddedWithOriginalArray<T>(List<T> newList, T[] originalArray)
    {
        return newList.Concat(
                originalArray
                    .Skip(newList.Count)
                    .Take(originalArray.Length - newList.Count))
            .ToList();
    }

    // Since the way arrays are formated involve a self-declared length followed by the content that has a fixed length,
    // it's possible that the advertised length is smaller than what was actually in the array. When this happens, the
    // game will ignore overflowed data even if it's physically still present in the asset. This was likely done as a
    // quicker way to delete data by making them unreadable instead of actually deleting them. For analysis purposes,
    // we would like to log any detected instances of these quirks in Trace level logs. We detect this by detecting non
    // default data past the advertised length.

    // This method does what was explained above with an int[] detecting non 0 values.
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

    // This method does what was explained above with a Vector3[] detecting non Vector3.zero values.
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
}