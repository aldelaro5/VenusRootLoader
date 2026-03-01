using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class EnemyEncounterCapTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    private static EnemyEncounterCapTopLevelPatcher _instance = null!;

    public EnemyEncounterCapTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _enemiesRegistry = enemiesRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(EnemyEncounterCapTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.SetVariables))]
    internal static IEnumerable<CodeInstruction> RemoveEnemyEncounterHardCap(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo enemyEncounterField = AccessTools.Field(typeof(MainManager), nameof(MainManager.enemyencounter));

        matcher.MatchStartForward(CodeMatch.StoresField(enemyEncounterField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Insert(Transpilers.EmitDelegate(GetNewEnemyEncounterCap));

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.Load))]
    internal static IEnumerable<CodeInstruction> AdjustEnemyEncounterAmountFromSave(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo enemyEncounterField = AccessTools.Field(typeof(MainManager), nameof(MainManager.enemyencounter));

        matcher.MatchStartForward(CodeMatch.StoresField(enemyEncounterField));
        matcher.MatchStartBackwards(Code.Ldlen);
        matcher.Advance(-1);
        matcher.MatchStartBackwards(Code.Ldlen);
        matcher.MatchStartForward(Code.Conv_I4);
        matcher.MatchStartForward(CodeMatch.LoadsLocal());
        matcher.Insert(Transpilers.EmitDelegate(GetBestEnemyEncounterAmountFromSave));

        return matcher.Instructions();
    }

    private static int GetNewEnemyEncounterCap(int originalCap) =>
        Math.Max(originalCap, _instance._enemiesRegistry.LeavesByNamedIds.Count);

    private static int GetBestEnemyEncounterAmountFromSave(int amountFromSaveFile) =>
        Math.Max(amountFromSaveFile, _instance._enemiesRegistry.LeavesByNamedIds.Count);
}