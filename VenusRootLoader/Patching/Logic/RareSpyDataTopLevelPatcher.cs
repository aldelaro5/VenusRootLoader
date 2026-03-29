using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for allowing <see cref="EnemyLeaf"/> to decide if they should be considered rare Spy Data when talking to Tattl.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="EventControl.Event65"/>: Changes the list of rare Spy Data enemy game ids to reflect the registry.</item>
/// </list>
/// </p>
/// </summary>
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
        MethodInfo listIntAddMethod = AccessTools.Method(typeof(List<int>), nameof(List<>.Add));

        // What we are essentially doing here is look for when the game adds the hardcoded GoldenSeedling to the list,
        // but then overwrite that logic with a delegate that clears the entire list and adds our own elements.
        // We do this because it's the last base game manipulation the game does, and it has to do this because the GoldenSeedling
        // isn't a boss / miniboss so we can sort of rely on this being there.
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