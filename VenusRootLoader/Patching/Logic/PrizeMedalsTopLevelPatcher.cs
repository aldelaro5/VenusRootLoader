using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for <see cref="PrizeMedalLeaf"/> since their data are normally hardcoded using the following arrays:
/// <list type="bullet">
/// <item><see cref="MainManager.prizeids"/>: The medal game ids.</item>
/// <item><see cref="MainManager.prizeflags"/>: The bound flagvar game ids.</item>
/// <item><see cref="MainManager.prizeenemyids"/>: The displayed enemy game ids.</item>
/// </list>
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetVariables"/>: Changes the 3 arrays to reflect the state of the prize medal registry.</item>
/// </list>
/// </p>
/// </summary>
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

    // We are essentially NOPing the creation of the array and replace it with an array we control.
    private static CodeMatcher PatchPrizeMedalArray(
        CodeMatcher matcher,
        FieldInfo arrayField,
        Func<int[]> arrayPatcher)
    {
        matcher.MatchStartForward(CodeMatch.StoresField(arrayField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        while (!matcher.Instruction.StoresField(arrayField))
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