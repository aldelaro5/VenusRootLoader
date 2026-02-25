using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class CrystalBerriesAmountTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<CrystalBerryLeaf> _crystalBerriesLeafRegistry;

    private static CrystalBerriesAmountTopLevelPatcher _instance = null!;

    public CrystalBerriesAmountTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesLeafRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _crystalBerriesLeafRegistry = crystalBerriesLeafRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(CrystalBerriesAmountTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.SetVariables))]
    internal static IEnumerable<CodeInstruction> RemoveCrystalBerriesHardCap(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo crystalBerriesFlagsField = AccessTools.Field(typeof(MainManager), nameof(MainManager.crystalbflags));

        matcher.MatchStartForward(CodeMatch.StoresField(crystalBerriesFlagsField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(GetNewCrystalBerriesCap));

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.Load))]
    internal static IEnumerable<CodeInstruction> AdjustCrystalBerriesAmountFromSave(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo crystalBerriesFlagsField = AccessTools.Field(typeof(MainManager), nameof(MainManager.crystalbflags));

        matcher.MatchStartForward(CodeMatch.StoresField(crystalBerriesFlagsField));
        matcher.MatchStartBackwards(Code.Newarr);
        matcher.Insert(Transpilers.EmitDelegate(GetBestCrystalBerriesAmountFromSave));

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.CheckAchievement))]
    internal static IEnumerable<CodeInstruction> AdjustCrystalBerriesRecordCheck(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        MethodInfo crystalBerriesAmountMethod = AccessTools.Method(
            typeof(MainManager),
            nameof(MainManager.CrystalBerryAmmount));

        matcher.MatchStartForward(CodeMatch.Calls(crystalBerriesAmountMethod));
        matcher.MatchStartForward(CodeMatch.LoadsConstant());
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(GetNewCrystalBerriesCap));

        return matcher.Instructions();
    }

    private static int GetNewCrystalBerriesCap() => _instance._crystalBerriesLeafRegistry.LeavesByNamedIds.Count;

    private static int GetBestCrystalBerriesAmountFromSave(int amountFromSaveFile) =>
        Math.Max(amountFromSaveFile, _instance._crystalBerriesLeafRegistry.LeavesByNamedIds.Count);
}