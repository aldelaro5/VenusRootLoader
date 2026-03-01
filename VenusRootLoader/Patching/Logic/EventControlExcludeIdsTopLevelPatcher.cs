using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class EventControlExcludeIdsTopLevelPatcher : ITopLevelPatcher
{
    private static EventControlExcludeIdsTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly IOrderedLeavesRegistry<EnemyLeaf> _orderedEnemiesRegistry;

    public EventControlExcludeIdsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        IOrderedLeavesRegistry<EnemyLeaf> orderedEnemiesRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _orderedEnemiesRegistry = orderedEnemiesRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(EventControlExcludeIdsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(EventControl), MethodType.Constructor)]
    private static IEnumerable<CodeInstruction> AdjustExcludeIds(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo excludeIdsField = AccessTools.Field(typeof(EventControl), nameof(EventControl.excludeids));

        matcher.MatchStartForward(CodeMatch.StoresField(excludeIdsField));
        matcher.MatchStartBackwards(Code.Newobj);
        matcher.Insert(Transpilers.EmitDelegate(GetNewExcludeIds));

        return matcher.Instructions();
    }

    private static IEnumerable<int> GetNewExcludeIds(IEnumerable<int> original)
    {
        List<EnemyLeaf> allEnemies = _instance._orderedEnemiesRegistry.Registry.LeavesByNamedIds.Values.ToList();
        List<EnemyLeaf> enemiesInBestiary = _instance._orderedEnemiesRegistry.GetOrderedLeaves().ToList();

        return allEnemies
            .Except(enemiesInBestiary)
            .Select(l => l.GameId);
    }
}