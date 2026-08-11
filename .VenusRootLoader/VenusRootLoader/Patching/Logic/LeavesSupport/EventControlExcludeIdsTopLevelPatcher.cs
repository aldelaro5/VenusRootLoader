using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="HasEnemyLeaf"/> registry to reflect which one is excluded or included in the
/// bestiary when speaking to Tattl (event 65).
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="EventControl()"/>: Changes <see cref="EventControl.excludeids"/> to reflect information from the
/// <see cref="HasEnemyLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class EventControlExcludeIdsTopLevelPatcher : ITopLevelPatcher
{
    private static EventControlExcludeIdsTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly IOrderedLeavesRegistry<HasEnemyLeaf> _orderedEnemiesRegistry;

    public EventControlExcludeIdsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        IOrderedLeavesRegistry<HasEnemyLeaf> orderedEnemiesRegistry)
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
        ILeavesRegistry<HasEnemyLeaf> allEnemies = _instance._orderedEnemiesRegistry.Registry;
        List<HasEnemyLeaf> enemiesInBestiary = _instance._orderedEnemiesRegistry.GetOrderedLeaves().ToList();

        return allEnemies
            .Except(enemiesInBestiary)
            .Select(l => l.GameId);
    }
}