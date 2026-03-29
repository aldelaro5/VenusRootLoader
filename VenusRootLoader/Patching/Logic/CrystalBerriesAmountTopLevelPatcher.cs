using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for a variable amount of <see cref="CrystalBerryLeaf"/> to exist in the game.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetVariables"/>: Changes the hardcoded length of <see cref="MainManager.crystalbflags"/>.</item>
/// <item><see cref="MainManager.Load"/>: Changes the length of <see cref="MainManager.crystalbflags"/> such that it picks the length of the registry if it exceeds the one read on the save file.</item>
/// <item><see cref="MainManager.CheckAchievement"/>: Changes the amount of needed <see cref="MainManager.crystalbflags"/> to be true for the Crystal Berries record to be unlocked.</item>
/// </list>
/// </p>
/// </summary>
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

        // The save loading process will actually attempt to overwrite the entire array with the lengths it got from the save.
        // We don't want this because it would undo our SetVariables patch so we want to patch this length. What's interesting
        // is this will not break save loading because Load will still use the length found in the actual save to read it.
        // What's actually going to happen is any leftover data will be left to default values, which is what we want.
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

    // The reason it needs to be the Math.Max between the 2 is because it's possible the save doesn't contain all the
    // Crystal Berries in the registry. In that case, we want to load the amount of the save, but the game will still
    // use the length from our registry and leave the values to false which is what we want.
    private static int GetBestCrystalBerriesAmountFromSave(int amountFromSaveFile) =>
        Math.Max(amountFromSaveFile, _instance._crystalBerriesLeafRegistry.LeavesByNamedIds.Count);
}