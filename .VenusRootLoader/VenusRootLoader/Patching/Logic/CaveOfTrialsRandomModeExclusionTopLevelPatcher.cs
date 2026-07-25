using HarmonyLib;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for allowing <see cref="EnemyLeaf"/> to decide if they should be included or excluded from
/// Cave Of Trials random mode pool.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="EventControl.GetRandomEnemy"/>: Replaces a while loop condition to only reject enemy game ids that the registry says to exclude.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class CaveOfTrialsRandomModeExclusionTopLevelPatcher : ITopLevelPatcher
{
    private static CaveOfTrialsRandomModeExclusionTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    public CaveOfTrialsRandomModeExclusionTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _enemiesRegistry = enemiesRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(CaveOfTrialsRandomModeExclusionTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(EventControl), nameof(EventControl.GetRandomEnemy))]
    private static IEnumerable<CodeInstruction> ChangeCaveOfTrialsRandomModeExclusions(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);

        matcher.MatchStartForward(CodeMatch.Branches());
        Label startLoopLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.LoadsLocal());
        int returnValueLocalIndex = matcher.Instruction.LocalIndex();
        CodeInstruction loadReturnValueLocal = CodeInstruction.LoadLocal(returnValueLocalIndex);
        matcher.MatchStartBackwards(CodeMatch.StoresLocal());
        matcher.Advance(1);
        // We are effectively hijacking the while condition here by going to the loop start if it's excluded and returning
        // immediately if it's included.
        matcher.Insert(
            loadReturnValueLocal,
            Transpilers.EmitDelegate(IsAllowedOnCaveOfTrialsRandomMode),
            new(OpCodes.Brfalse, startLoopLabel),
            loadReturnValueLocal,
            new(OpCodes.Ret));

        return matcher.Instructions();
    }

    private static bool IsAllowedOnCaveOfTrialsRandomMode(int enemyId) =>
        _instance._enemiesRegistry.LeavesByGameIds[enemyId].IsIncludedInRandomCaveOfTrialsPool;
}