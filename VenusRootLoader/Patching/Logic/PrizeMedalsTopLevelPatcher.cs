using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class PrizeMedalsTopLevelPatcher : ITopLevelPatcher
{
    private static PrizeMedalsTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<PrizeMedalLeaf> _prizeMedalsRegistry;

    public PrizeMedalsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<PrizeMedalLeaf> prizeMedalsRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _prizeMedalsRegistry = prizeMedalsRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(PrizeMedalsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.SetVariables))]
    internal static IEnumerable<CodeInstruction> PatchPrizeMedalsData(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);

        FieldInfo prizeIdsField = AccessTools.Field(typeof(MainManager), nameof(MainManager.prizeids));
        matcher = PatchPrizeMedalArray(matcher, prizeIdsField, GetPrizeIds);
        FieldInfo prizeFlagsField = AccessTools.Field(typeof(MainManager), nameof(MainManager.prizeflags));
        matcher = PatchPrizeMedalArray(matcher, prizeFlagsField, GetPrizeFlagIds);
        FieldInfo prizeEnemyIdsField = AccessTools.Field(typeof(MainManager), nameof(MainManager.prizeenemyids));
        matcher = PatchPrizeMedalArray(matcher, prizeEnemyIdsField, GetPrizeEnemyIds);

        return matcher.Instructions();
    }

    private static CodeMatcher PatchPrizeMedalArray(
        CodeMatcher matcher,
        FieldInfo prizeIdsField,
        Func<int[]> arrayPatcher)
    {
        matcher.MatchStartForward(CodeMatch.StoresField(prizeIdsField))
            .MatchStartBackwards(CodeMatch.LoadsConstant());
        while (!matcher.Instruction.StoresField(prizeIdsField))
            matcher.SetInstructionAndAdvance(Code.Nop);
        matcher.Advance(-1);
        matcher.SetInstruction(Transpilers.EmitDelegate(arrayPatcher));
        matcher.Start();
        return matcher;
    }

    private static int[] GetPrizeIds() =>
        _instance._prizeMedalsRegistry.LeavesByNamedIds.Values.Select(l => l.MedalGameId).ToArray();

    private static int[] GetPrizeFlagIds() =>
        _instance._prizeMedalsRegistry.LeavesByNamedIds.Values.Select(l => l.FlagvarGameId).ToArray();

    private static int[] GetPrizeEnemyIds() =>
        _instance._prizeMedalsRegistry.LeavesByNamedIds.Values.Select(l => l.DisplayedEnemyGameId).ToArray();
}