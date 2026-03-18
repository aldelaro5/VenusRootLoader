using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

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
    internal static IEnumerable<CodeInstruction> RemoveCrystalBerriesHardCap(
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
        CodeInstruction mapIdLdLoc = new(OpCodes.Ldloc_S, mapIdLocalIndex);
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(CodeMatch.LoadsLocal());
        CodeInstruction mapEntityIdLdLoc = matcher.Instruction.Clone();
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlEventIdField));
        matcher.MatchStartForward(CodeMatch.StoresLocal());
        LocalBuilder mapEntityDataFieldIndexLocal = (LocalBuilder)matcher.Instruction.operand;
        matcher.Advance(1);

        matcher.MatchStartForward(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLdLoc, mapIdLdLoc, Transpilers.EmitDelegate(PatchNewRequiresLength));

        matcher.MatchStartForward(CodeMatch.StoresField(npcControlLimitField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlLimitField));
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLdLoc, mapIdLdLoc, Transpilers.EmitDelegate(PatchNewLimitsLength));

        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlDataField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlDataField));
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLdLoc, mapIdLdLoc, Transpilers.EmitDelegate(PatchNewDataLength));

        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlVectorDataField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlVectorDataField));
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLdLoc, mapIdLdLoc, Transpilers.EmitDelegate(PatchNewVectorDataLength));

        matcher.MatchStartForward(CodeMatch.StoresField(npcControlDialoguesField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlDialoguesField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlDialoguesField));
        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLdLoc, mapIdLdLoc, Transpilers.EmitDelegate(PatchNewDialoguesLength));

        matcher.MatchStartForward(CodeMatch.StoresField(npcControlBattleIdsField));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.StoresField(npcControlBattleIdsField));
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldelem_Ref));
        matcher.MatchStartBackwards(new CodeMatch(i => i.IsStloc(mapEntityDataFieldIndexLocal)));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLdLoc, mapIdLdLoc, Transpilers.EmitDelegate(PatchNewBattleIdsLength));

        matcher.MatchStartForward(CodeMatch.StoresField(npcControlEmoticonFlagsField));
        matcher.MatchStartForward(CodeMatch.Branches());
        Label emoticonFlagLoopLabel = (Label)matcher.Instruction.operand;
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(emoticonFlagLoopLabel)));
        matcher.MatchStartForward(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(mapEntityIdLdLoc, mapIdLdLoc, Transpilers.EmitDelegate(PatchNewEmoticonFlagsLength));

        return matcher.Instructions();
    }

    private static int PatchNewRequiresLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].Requires.Length);
    }

    private static int PatchNewLimitsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].Limit.Length);
    }

    private static int PatchNewDataLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].Data.Length);
    }

    private static int PatchNewVectorDataLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].VectorData.Length * 3);
    }

    private static int PatchNewDialoguesLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].Dialogues.Length * 3);
    }

    private static int PatchNewBattleIdsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].BattleEnemyIds.Length);
    }

    private static int PatchNewEmoticonFlagsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.LeavesByGameIds[mapGameId];
        return Math.Max(originalLength, mapLeaf.Entities[mapEntityId].EmoticonFlags.Length);
    }
}