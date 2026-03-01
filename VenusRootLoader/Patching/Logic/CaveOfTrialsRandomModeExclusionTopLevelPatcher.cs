using HarmonyLib;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

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
        OpCode opCodelocalReturnValue = matcher.Opcode;
        matcher.MatchStartBackwards(CodeMatch.StoresLocal());
        matcher.Advance(1);
        matcher.Insert(
            new(opCodelocalReturnValue),
            Transpilers.EmitDelegate(IsAllowedOnCaveOfTrialsRandomMode),
            new(OpCodes.Brfalse, startLoopLabel),
            new(opCodelocalReturnValue),
            new(OpCodes.Ret));

        return matcher.Instructions();
    }

    private static bool IsAllowedOnCaveOfTrialsRandomMode(int enemyId) =>
        _instance._enemiesRegistry.LeavesByGameIds[enemyId].IsIncludedInRandomCaveOfTrialsPool;
}