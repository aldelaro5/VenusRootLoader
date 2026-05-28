using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities;
using VenusRootLoader.Api.MapEntities.Enemies;
using VenusRootLoader.Api.MapEntities.Npcs;
using VenusRootLoader.Api.MapEntities.Objects;
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

    public string GetTextAssetSerializedString(string subPath, MapEntity mapEntity)
    {
        if (subPath.EndsWith("names", StringComparison.OrdinalIgnoreCase))
            return mapEntity.BaseGameObjectName;

        StringBuilder sb = new();

        sb.Append(mapEntity.Type.ToString());
        sb.Append('}');
        sb.Append(mapEntity.ObjectType.ToString());
        sb.Append('}');
        sb.Append(mapEntity.InternalPrimaryBehavior.ToString());
        sb.Append('}');
        sb.Append(mapEntity.InternalSecondaryBehavior.ToString());
        sb.Append('}');
        sb.Append(mapEntity.Interaction.ToString());
        sb.Append('}');
        sb.Append(mapEntity.InternalDeathType.ToString());
        sb.Append('}');
        sb.Append(mapEntity.InternalStartingPosition.x);
        sb.Append('}');
        sb.Append(mapEntity.InternalStartingPosition.y);
        sb.Append('}');
        sb.Append(mapEntity.InternalStartingPosition.z);
        sb.Append('}');
        sb.Append(mapEntity.InternalAnimIdOrItemId);
        sb.Append('}');
        sb.Append(mapEntity.InternalIsFlipped);
        sb.Append('}');
        sb.Append(mapEntity.InternalCcolHeight);
        sb.Append('}');
        sb.Append(mapEntity.InternalCcolRadius);
        sb.Append('}');
        sb.Append(mapEntity.InternalRadius);
        sb.Append('}');
        sb.Append(mapEntity.InternalTimer);
        sb.Append('}');
        sb.Append(mapEntity.InternalSpeed);
        sb.Append('}');
        sb.Append(mapEntity.InternalPrimaryActionFrequency);
        sb.Append('}');
        sb.Append(mapEntity.InternalSecondaryActionFrequency);
        sb.Append('}');
        sb.Append(mapEntity.InternalSpeedMultiplier);
        sb.Append('}');
        sb.Append(mapEntity.InternalRadiusLimit);
        sb.Append('}');
        sb.Append(mapEntity.InternalWanderRadius);
        sb.Append('}');
        sb.Append(mapEntity.InternalTeleportRadius);
        sb.Append('}');
        sb.Append(mapEntity.InternalHaxBoxCol);
        sb.Append('}');
        sb.Append(mapEntity.InternalBoxColIsTrigger);
        sb.Append('}');
        sb.Append(mapEntity.InternalBoxColSize.x);
        sb.Append('}');
        sb.Append(mapEntity.InternalBoxColSize.y);
        sb.Append('}');
        sb.Append(mapEntity.InternalBoxColSize.z);
        sb.Append('}');
        sb.Append(mapEntity.InternalBoxColCenter.x);
        sb.Append('}');
        sb.Append(mapEntity.InternalBoxColCenter.y);
        sb.Append('}');
        sb.Append(mapEntity.InternalBoxColCenter.z);
        sb.Append('}');
        sb.Append(mapEntity.InternalFreezeTime);
        sb.Append('}');
        sb.Append(mapEntity.InternalFreezeSize.x);
        sb.Append('}');
        sb.Append(mapEntity.InternalFreezeSize.y);
        sb.Append('}');
        sb.Append(mapEntity.InternalFreezeSize.z);
        sb.Append('}');
        sb.Append(mapEntity.InternalFreezeOffset.x);
        sb.Append('}');
        sb.Append(mapEntity.InternalFreezeOffset.y);
        sb.Append('}');
        sb.Append(mapEntity.InternalFreezeOffset.z);
        sb.Append('}');
        sb.Append(
            mapEntity is { Type: NPCControl.NPCType.NPC, Interaction: NPCControl.Interaction.LockedDoor }
                ? 0
                : mapEntity.InternalEventId);
        sb.Append('}');

        sb.Append(mapEntity.InternalRequires.Count);
        sb.Append('}');

        List<int> allRequires = GetListPaddedWithOriginalArray(
            mapEntity.InternalRequires.Select(r => r.GameId).ToList(),
            mapEntity.OriginalRequires);
        foreach (int require in allRequires)
        {
            sb.Append(require);
            sb.Append('}');
        }

        sb.Append(mapEntity.InternalLimits.Count);
        sb.Append('}');

        List<int> allLimitsValues = mapEntity.InternalLimits
            .Select(l => l.FailsWholeConditionWhenFlagIsTrue
                ? -l.Flag.GameId
                : l.Flag.GameId)
            .ToList();
        List<int> allLimits = GetListPaddedWithOriginalArray(allLimitsValues, mapEntity.OriginalLimits);
        foreach (int limit in allLimits)
        {
            sb.Append(limit);
            sb.Append('}');
        }

        sb.Append(mapEntity.InternalData.Count);
        sb.Append('}');

        List<int> allData = GetListPaddedWithOriginalArray(mapEntity.InternalData, mapEntity.OriginalData);
        foreach (int data in allData)
        {
            sb.Append(data);
            sb.Append('}');
        }

        sb.Append(mapEntity.InternalVectorData.Count);
        sb.Append('}');

        List<Vector3> allVectorData =
            GetListPaddedWithOriginalArray(mapEntity.InternalVectorData, mapEntity.OriginalVectorData);
        foreach (Vector3 vectorData in allVectorData)
        {
            sb.Append(vectorData.x);
            sb.Append('}');
            sb.Append(vectorData.y);
            sb.Append('}');
            sb.Append(vectorData.z);
            sb.Append('}');
        }

        mapEntity.InternalSecondaryVectorDataArray = mapEntity.InternalSecondaryVectorData.ToArray();

        sb.Append(
            mapEntity is { Type: NPCControl.NPCType.NPC, Interaction: NPCControl.Interaction.Shop }
                ? 1
                : mapEntity.InternalDialogues.Count);
        sb.Append('}');

        List<Vector3> allDialogues = GetListPaddedWithOriginalArray(
            mapEntity.InternalDialogues,
            mapEntity.OriginalDialogues);
        foreach (Vector3 dialogue in allDialogues)
        {
            sb.Append(dialogue.x);
            sb.Append('}');
            sb.Append(dialogue.y);
            sb.Append('}');
            sb.Append(dialogue.z);
            sb.Append('}');
        }

        sb.Append(mapEntity.InternalEulerAngles.x);
        sb.Append('}');
        sb.Append(mapEntity.InternalEulerAngles.y);
        sb.Append('}');
        sb.Append(mapEntity.InternalEulerAngles.z);
        sb.Append('}');

        sb.Append(mapEntity.InternalBattleEnemyIds.Count);
        sb.Append('}');

        List<int> allBattleEnemyIds =
            GetListPaddedWithOriginalArray(mapEntity.InternalBattleEnemyIds, mapEntity.OriginalBattleEnemyIds);
        foreach (int battleEnemyId in allBattleEnemyIds)
        {
            sb.Append(battleEnemyId);
            sb.Append('}');
        }

        sb.Append(mapEntity.TagColor.r);
        sb.Append('}');
        sb.Append(mapEntity.TagColor.g);
        sb.Append('}');
        sb.Append(mapEntity.TagColor.b);
        sb.Append('}');
        sb.Append(mapEntity.TagColor.a);
        sb.Append('}');
        sb.Append(mapEntity.InternalEmoticonOffset.x);
        sb.Append('}');
        sb.Append(mapEntity.InternalEmoticonOffset.y);
        sb.Append('}');
        sb.Append(mapEntity.InternalEmoticonOffset.z);
        sb.Append('}');
        sb.Append(mapEntity.InsideId);
        sb.Append('}');

        List<Vector2> allEmoticonFlags =
            GetListPaddedWithOriginalArray(mapEntity.InternalEmoticonFlags, mapEntity.OriginalEmoticonFlags);
        foreach (Vector2 emoticonFlag in allEmoticonFlags)
        {
            sb.Append(emoticonFlag.x);
            sb.Append(',');
            sb.Append(emoticonFlag.y);
            sb.Append('}');
        }

        sb.Append(mapEntity.InternalSpyDialogueMapId);
        sb.Append('}');
        sb.Append(mapEntity.InternalRegionalFlagId);
        sb.Append('}');
        sb.Append(mapEntity.InitialHeight);
        sb.Append('}');
        sb.Append(mapEntity.InternalBobRange);
        sb.Append('}');
        sb.Append(mapEntity.InternalBobSpeed);
        sb.Append('}');
        sb.Append(mapEntity.InternalActivationFlagId);
        sb.Append('}');

        string valueReturnToHeightString;
        if (mapEntity.IsReturnToHeightOriginallyInt)
        {
            int returnToHeightInt = mapEntity.ReturnToHeight ? 1 : 0;
            valueReturnToHeightString = returnToHeightInt.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            valueReturnToHeightString = mapEntity.ReturnToHeight.ToString();
        }

        sb.Append(valueReturnToHeightString);

        if (!string.IsNullOrEmpty(mapEntity.UnusedOverflowData))
        {
            sb.Append('}');
            sb.Append(mapEntity.UnusedOverflowData);
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
        MapEntity value = GetTypedMapEntity(
            map,
            id,
            baseGameId,
            type,
            objectType,
            interaction,
            primaryBehavior,
            secondaryBehavior,
            fields);

        value.Id = id;
        value.BaseGameObjectName = name;
        value.Map = map;
        value.OriginalType = type;
        value.OriginalObjectType = objectType;
        value.OriginalInteraction = interaction;
        value.InternalPrimaryBehavior = primaryBehavior;
        value.InternalSecondaryBehavior = secondaryBehavior;
        value.InternalDeathType = Enum.Parse<NPCControl.DeathType>(fields[5]);
        value.InternalStartingPosition = new(float.Parse(fields[6]), float.Parse(fields[7]), float.Parse(fields[8]));
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
            value.InternalRequires.Add(new(_flagsRegistry.LeavesByGameIds[value.OriginalRequires[i]]));

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.BaseGameObjectName,
                nameof(MapEntity.InternalRequires),
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
                nameof(MapEntity.InternalLimits),
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
        value.InternalSecondaryVectorData.AddRange(value.InternalVectorData);

        if (type is NPCControl.NPCType.NPC or NPCControl.NPCType.Enemy &&
            (BehaviorsWithSecondaryVectorData.Contains(primaryBehavior) ||
             BehaviorsWithSecondaryVectorData.Contains(secondaryBehavior)))
        {
            value.InternalVectorData.Clear();
        }

        if (value is not EnemyEncounterHoldingKeyItemMapEntity &&
            (BehaviorsWithSecondaryVectorData.Contains(primaryBehavior) ||
             BehaviorsWithSecondaryVectorData.Contains(secondaryBehavior)))
        {
            value.InternalVectorData.Clear();
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogIfListHasUnreadableData(
                value.BaseGameObjectName,
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
                nameof(MapEntity.InternalDialogues),
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

        for (int i = 0; i < 10; i++)
        {
            string[] components = fields[179 + i].Split(StringUtils.CommaSplitDelimiter);
            value.OriginalEmoticonFlags[i] = new(float.Parse(components[0]), float.Parse(components[1]));

            if (i == 0 || value.OriginalEmoticonFlags[i].x > 0)
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
    }

    private MapEntity GetTypedMapEntity(
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
        ILeavesRegistry<MapEntity> registry = map.EntitiesRegistry;
        string namedId = id.ToString();

        return (type, objectType, interaction) switch
        {
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.BeetleGrass, _) =>
                registry.RegisterExisting<CuttableGrassMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.PushRock, _) =>
                int.Parse(fields[60]) < 3 ||
                int.Parse(fields[61 + 2]) == 0
                    ? registry.RegisterExisting<MovableRockMapEntity>(id, namedId, baseGameId)
                    : registry.RegisterExisting<SlidingIcePillarMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.PressurePlate, _) =>
                registry.RegisterExisting<PressurePlateMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.ANDGate, _) =>
                int.Parse(fields[60]) == 2 && int.Parse(fields[61 + 1]) == -1
                    ? registry.RegisterExisting<AndGateOnSingleFlagMapEntity>(id, namedId, baseGameId)
                    : int.Parse(fields[61 + 0]) switch
                    {
                        -2 => registry.RegisterExisting<AndGateOnFlagsMapEntity>(id, namedId, baseGameId),
                        >= -1 => registry.RegisterExisting<AndGateOnEntitiesActivationMapEntity>(
                            id,
                            namedId,
                            baseGameId),
                        _ => ThrowHelper.ThrowArgumentOutOfRangeException<MapEntity>()
                    },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.CameraChange, _) => registry.RegisterExisting<
                CameraChangeMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.Item, _) => int.Parse(fields[61 + 0]) switch
            {
                0 or 1 => registry.RegisterExisting<CollectibleItemMapEntity>(id, namedId, baseGameId),
                2 => registry.RegisterExisting<CollectibleMedalMapEntity>(id, namedId, baseGameId),
                3 => registry.RegisterExisting<CollectibleCrystalBerryMapEntity>(id, namedId, baseGameId),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<MapEntity>()
            },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.DoorOtherMap, _) =>
                registry.RegisterExisting<LoadingZoneMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.SetPlayerRespawn, _) =>
                registry.RegisterExisting<SetPlayerRespawnZoneMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.DoorSameMap, _) =>
                registry.RegisterExisting<InsideTransitionZoneMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.EventTrigger, _) =>
                int.Parse(fields[60]) >= 3 &&
                int.Parse(fields[61 + 2]) == 1
                    ? registry.RegisterExisting<AutomaticEventTriggerMapEntity>(id, namedId, baseGameId)
                    : registry.RegisterExisting<EventTriggerZoneMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.DialogueTrigger, _) =>
                int.Parse(fields[60]) >= 3 &&
                int.Parse(fields[61 + 2]) == 1
                    ? registry.RegisterExisting<AutomaticMapDialogueTriggerMapEntity>(id, namedId, baseGameId)
                    : registry.RegisterExisting<MapDialogueTriggerZoneMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.ANDBlock, _) =>
                int.Parse(fields[60]) == 2 && int.Parse(fields[61 + 1]) == -1
                    ? registry.RegisterExisting<AndBlockOnSingleFlagMapEntity>(id, namedId, baseGameId)
                    : int.Parse(fields[61 + 0]) switch
                    {
                        -2 => registry.RegisterExisting<AndBlockOnFlagsMapEntity>(id, namedId, baseGameId),
                        >= -1 => registry.RegisterExisting<AndBlockOnEntitiesActivationMapEntity>(
                            id,
                            namedId,
                            baseGameId),
                        _ => ThrowHelper.ThrowArgumentOutOfRangeException<MapEntity>()
                    },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.SavePoint, _) =>
                int.Parse(fields[61 + 1]) >= 10
                    ? registry.RegisterExisting<DeadLanderOmegaAlertCrystalMapEntity>(id, namedId, baseGameId)
                    : registry.RegisterExisting<SavePointCrystalMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.JumpSpring, _) =>
                int.Parse(fields[61 + 0]) == 1
                    ? registry.RegisterExisting<JumpToPositionSpringMapEntity>(id, namedId, baseGameId)
                    : registry.RegisterExisting<JumpUpSpringMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.DigSpot, _) =>
                (int.Parse(fields[61 + 0]), int.Parse(fields[61 + 1])) switch
                {
                    (1, _) => registry.RegisterExisting<DigSpotCrystalBerryMapEntity>(id, namedId, baseGameId),
                    (>= 2, _) => registry.RegisterExisting<DigSpotStartEventMapEntity>(id, namedId, baseGameId),
                    (<= 0, >= 2) => registry.RegisterExisting<DigSpotMedalMapEntity>(id, namedId, baseGameId),
                    (<= 0, < 2) => registry.RegisterExisting<DigSpotItemMapEntity>(id, namedId, baseGameId),
                },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.Switch, _) =>
                (int.Parse(fields[61 + 0]), int.Parse(fields[61 + 1]), int.Parse(fields[61 + 2])) switch
                {
                    (0, 0, 0) => registry.RegisterExisting<LatchedSwitchMapEntity>(id, namedId, baseGameId),
                    (0, 0, _) => registry.RegisterExisting<TimerSwitchMapEntity>(id, namedId, baseGameId),
                    (0, 1, _) => registry.RegisterExisting<LinkableToggleSwitchMapEntity>(id, namedId, baseGameId),
                    (1, >= 0, _) => registry.RegisterExisting<EventTriggerSwitchMapEntity>(id, namedId, baseGameId),
                    _ => ThrowHelper.ThrowArgumentOutOfRangeException<MapEntity>()
                },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.CoiledObject, _) =>
                registry.RegisterExisting<TrappedEntityMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.FixedAnim, _) =>
                registry.RegisterExisting<FixedAnimstateMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.EnemySpawner, _) =>
                registry.RegisterExisting<EnemySpawnerMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.Dropplet, _) =>
                registry.RegisterExisting<FreezableWaterDropletMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.PathPlatform, _) =>
                (int)float.Parse(fields[103 + (1 * 3) + 0]) == 1
                    ? registry.RegisterExisting<MovingPlatformAlongLerpMapEntity>(id, namedId, baseGameId)
                    : registry.RegisterExisting<MovingPlatformAlongPathMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.BreakableRock, _) =>
                registry.RegisterExisting<BreakableRockMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.RotatingPlatform, _) =>
                registry.RegisterExisting<RotatingPlatformMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.Geizer, _) =>
                registry.RegisterExisting<GeyserMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.MusicRange, _) =>
                registry.RegisterExisting<MusicChangeZoneMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.TempPlatform, _) =>
                registry.RegisterExisting<FlytrapPlatformMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.ScrewSwitch, _) =>
                registry.RegisterExisting<SpinningCrankMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.ResetCamera, _) =>
                registry.RegisterExisting<ResetCameraZoneMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.StencilSwitch, _) =>
                registry.RegisterExisting<IceRadiusSwitchMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.RollingRock, _) => int.Parse(fields[61 + 2]) switch
            {
                1 => registry.RegisterExisting<RollingRockCanonMapEntity>(id, namedId, baseGameId),
                _ => registry.RegisterExisting<RollingRockMapEntity>(id, namedId, baseGameId)
            },
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.TriggerSwitch, _) =>
                registry.RegisterExisting<SwitchTriggerZoneMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.WindPusher, _) =>
                registry.RegisterExisting<WindBeamZoneMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Object, NPCControl.ObjectTypes.WaterSwitch, _) =>
                registry.RegisterExisting<MapChildVerticalPositionSwitchMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.Enemy, _, _) => (int)float.Parse(fields[72 + (0 * 3) + 1]) == -2 &&
                                                !BehaviorsWithSecondaryVectorData.Contains(primaryBehavior) &&
                                                !BehaviorsWithSecondaryVectorData.Contains(secondaryBehavior)
                ? registry.RegisterExisting<EnemyEncounterHoldingKeyItemMapEntity>(id, namedId, baseGameId)
                : registry.RegisterExisting<EnemyEncounterWithRegularItemDropsMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.None) =>
                registry.RegisterExisting<NoInteractionNpcMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.Talk or NPCControl.Interaction.Check) =>
                registry.RegisterExisting<TalkingNpcMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.Event or NPCControl.Interaction.LockedDoor) =>
                registry.RegisterExisting<EventNpcMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.Shop) => float.Parse(fields[103 + (10 * 3)] + 0) == 0f
                ? registry.RegisterExisting<ItemsShopMapEntity>(id, namedId, baseGameId)
                : registry.RegisterExisting<MedalsShopMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.QuestBoard) =>
                registry.RegisterExisting<QuestBoardNpcMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.StorageAnt) =>
                registry.RegisterExisting<ItemsStorageNpcMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.CaravanBadge) =>
                registry.RegisterExisting<CaravanShelvedMedalNpcMapEntity>(id, namedId, baseGameId),
            (NPCControl.NPCType.NPC, _, NPCControl.Interaction.VenusHeal) =>
                registry.RegisterExisting<VenusHealingNpcMapEntity>(id, namedId, baseGameId),
            _ => ThrowHelper.ThrowInvalidOperationException<MapEntity>(
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