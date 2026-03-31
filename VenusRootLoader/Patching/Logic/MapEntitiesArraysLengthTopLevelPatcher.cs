using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for a variable amount of elements in array data present on map entities to exist in the game.
/// More specifically, it allows custom length for these fields:
/// <list type="bullet">
/// <item><see cref="NPCControl.requires"/></item>
/// <item><see cref="NPCControl.limit"/></item>
/// <item><see cref="NPCControl.data"/></item>
/// <item><see cref="NPCControl.vectordata"/></item>
/// <item><see cref="NPCControl.dialogues"/></item>
/// <item><see cref="NPCControl.battleids"/></item>
/// <item><see cref="NPCControl.emoticonflag"/></item>
/// </list>
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MapControl.CreateEntities"/>: Changes the hardcoded length of the array fields to accomodate our map entities.</item>
/// <item><see cref="NPCControl.SetBadgeShop"/>: Changes the amount of shelved items to be the length of
/// <see cref="NPCControl.vectordata"/> instead of <see cref="NPCControl.data"/> which is easier to work with because
/// the content of <see cref="NPCControl.data"/> isn't used for a medals shops while <see cref="NPCControl.vectordata"/>
/// is used for both types of shops.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class MapEntitiesArraysLengthTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<MapLeaf> _mapsLeafRegistry;

    private static MapEntitiesArraysLengthTopLevelPatcher _instance = null!;

    public MapEntitiesArraysLengthTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<MapLeaf> mapsLeafRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _mapsLeafRegistry = mapsLeafRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(MapEntitiesArraysLengthTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MapControl), nameof(MapControl.CreateEntities))]
    internal static IEnumerable<CodeInstruction> RemoveMapEntitiesArraysLengthHardCap(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo npcControlEventIdField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.eventid));
        FieldInfo npcControlLimitField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.limit));
        FieldInfo npcControlDataField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.data));
        FieldInfo npcControlVectorDataField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.vectordata));
        FieldInfo npcControlDialoguesField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.dialogues));
        FieldInfo npcControlBattleIdsField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.battleids));
        FieldInfo npcControlEmoticonFlagsField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.emoticonflag));

        matcher.MatchStartForward(CodeMatch.StoresLocal());
        int mapIdLocalIndex = matcher.Instruction.LocalIndex();
        CodeInstruction mapIdLocalLoad = CodeInstruction.LoadLocal(mapIdLocalIndex);

        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(CodeMatch.LoadsLocal());
        int mapEntityIdLocalIndex = matcher.Instruction.LocalIndex();
        CodeInstruction mapEntityIdLoadLocal = CodeInstruction.LoadLocal(mapEntityIdLocalIndex);

        // This field is right before the first we need to patch so it's convenient to go to its store first.
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlEventIdField));
        matcher.MatchStartForward(CodeMatch.StoresLocal());
        LocalBuilder mapEntityDataFieldIndexLocal = (LocalBuilder)matcher.Instruction.operand;
        matcher.Advance(1);

        matcher.MatchStartForward(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLoadLocal, mapIdLocalLoad, Transpilers.EmitDelegate(PatchNewRequiresLength));

        matcher.MatchStartForward(CodeMatch.StoresField(npcControlLimitField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlLimitField));
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLoadLocal, mapIdLocalLoad, Transpilers.EmitDelegate(PatchNewLimitsLength));

        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlDataField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlDataField));
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLoadLocal, mapIdLocalLoad, Transpilers.EmitDelegate(PatchNewDataLength));

        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlVectorDataField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlVectorDataField));
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLoadLocal, mapIdLocalLoad, Transpilers.EmitDelegate(PatchNewVectorDataFieldsLength));

        // This one patches the hardcoded length if the map entity is a shop system
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlDialoguesField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLoadLocal, mapIdLocalLoad, Transpilers.EmitDelegate(PatchNewDialoguesLength));

        // Still a shop system, but this one patches the for loop length condition
        matcher.MatchStartForward(CodeMatch.Branches());
        Label labelLoopEnd = (Label)matcher.Instruction.operand;
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(labelLoopEnd)));
        matcher.MatchStartForward(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLoadLocal, mapIdLocalLoad, Transpilers.EmitDelegate(PatchNewVectorDataFieldsLength));

        // This one patches the accunukated field index
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlDialoguesField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlDialoguesField));
        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLoadLocal, mapIdLocalLoad, Transpilers.EmitDelegate(PatchNewDialoguesFieldsLength));

        matcher.MatchStartForward(CodeMatch.StoresField(npcControlBattleIdsField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlBattleIdsField));
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLoadLocal, mapIdLocalLoad, Transpilers.EmitDelegate(PatchNewBattleIdsLength));

        matcher.MatchStartForward(CodeMatch.StoresField(npcControlEmoticonFlagsField));
        matcher.MatchStartForward(CodeMatch.Branches());
        Label emoticonFlagLoopLabel = (Label)matcher.Instruction.operand;
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(emoticonFlagLoopLabel)));
        matcher.MatchStartForward(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLoadLocal, mapIdLocalLoad, Transpilers.EmitDelegate(PatchNewEmoticonFlagsLength));

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(NPCControl), nameof(NPCControl.SetBadgeShop))]
    internal static IEnumerable<CodeInstruction> FixSetBadgeShopPoolMedalsLength(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo npcControlDataField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.data));
        FieldInfo npcControlVectorDataField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.vectordata));

        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlDataField));
        matcher.Instruction.operand = npcControlVectorDataField;
        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlDataField));
        matcher.Instruction.operand = npcControlVectorDataField;

        return matcher.Instructions();
    }

    private static int PatchNewRequiresLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].Requires.Count);
    }

    private static int PatchNewLimitsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].Limit.Count);
    }

    private static int PatchNewDataLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].InternalData.Count);
    }

    private static int PatchNewVectorDataFieldsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].InternalVectorData.Count * 3);
    }

    private static int PatchNewDialoguesLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].InternalDialogues.Count);
    }

    // This is the amount of fields, the one above is the array length.
    private static int PatchNewDialoguesFieldsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].InternalDialogues.Count * 3);
    }

    private static int PatchNewBattleIdsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].InternalBattleEnemyIds.Count);
    }

    private static int PatchNewEmoticonFlagsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].InternalEmoticonFlags.Count);
    }
}