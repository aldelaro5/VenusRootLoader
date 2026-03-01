using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class RareSpyDataTopLevelPatcher : ITopLevelPatcher
{
    private static RareSpyDataTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    public RareSpyDataTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _enemiesRegistry = enemiesRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(RareSpyDataTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(EventControl), nameof(EventControl.Event65), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> ChangeRareSpyDataList(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        MethodInfo listIntAddMethod = AccessTools.Method(typeof(List<int>), nameof(List<int>.Add));

        matcher.MatchStartForward(CodeMatch.Calls(listIntAddMethod));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(ChangeRareSpyDataEnemies));
        while (matcher.Opcode != OpCodes.Ldarg_0)
            matcher.SetInstructionAndAdvance(Code.Nop);

        return matcher.Instructions();
    }

    private static void ChangeRareSpyDataEnemies(List<int> rareSpyDataEnemyIds)
    {
        rareSpyDataEnemyIds.Clear();
        List<int> newRareSpyDateEnemyIds = _instance._enemiesRegistry.LeavesByNamedIds.Values
            .Where(l => l.IsRareSpyData)
            .Select(l => l.GameId)
            .ToList();
        rareSpyDataEnemyIds.AddRange(newRareSpyDateEnemyIds);
    }
}