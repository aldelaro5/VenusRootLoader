using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher improves the memory allocation performance of the game by changing <see cref="MainManager.CheckIfCanExist"/>
/// so it doesn't allocate any memory, and it also changes <see cref="MainManager.DoClock"/> to not prematurely cause Garbage collections.
/// These changes were shown using the Unity profiler to be very effective at reducing stutters which would get worse in <see cref="VenusRootLoader"/>
/// because of the increased complexity of a <see cref="GC.Collect()"/> call.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.CheckIfCanExist"/>: Fixes an unnecessary heap allocation.</item>
/// <item><see cref="MainManager.DoClock"/>: Removes a forced GC collection which becomes less useful due to the above fix.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class MemoryAllocationsTopLevelPatcher : ITopLevelPatcher
{
    private static readonly List<int> TemporaryLimits = new();

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;

    public MemoryAllocationsTopLevelPatcher(IHarmonyTypePatcher harmonyTypePatcher)
    {
        _harmonyTypePatcher = harmonyTypePatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(MemoryAllocationsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.DoClock))]
    internal static IEnumerable<CodeInstruction> RemoveForcedGcCollection(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo clockSecField = AccessTools.Field(typeof(MainManager), nameof(MainManager.clocksec));
        MethodInfo gcCollectMethod = AccessTools.Method(typeof(GC), nameof(GC.Collect), []);

        matcher.MatchStartForward(CodeMatch.StoresField(clockSecField));
        matcher.Advance(1);
        while (!matcher.Instruction.Calls(gcCollectMethod))
            matcher.SetInstructionAndAdvance(Code.Nop);
        matcher.SetInstructionAndAdvance(Code.Nop);

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.CheckIfCanExist))]
    internal static IEnumerable<CodeInstruction> FixMemoryAllocation(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        MethodInfo checkAllBoolMethod = AccessTools.Method(
            typeof(MainManager),
            nameof(MainManager.CheckAllBool),
            [typeof(bool[]), typeof(int[]), typeof(bool)]);

        matcher.MatchStartForward(Code.Newobj);
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(GetClearedPreallocatedList));
        int localIndex = matcher.Instruction.LocalIndex();
        matcher.MatchStartForward(CodeMatch.Branches());
        Label labelForConditionCheck = (Label)matcher.Operand;
        matcher.MatchStartForward(new CodeMatch(x => x.labels.Contains(labelForConditionCheck)));
        matcher.MatchStartForward(new CodeMatch(x => x.IsLdloc() && x.LocalIndex() == localIndex));
        while (!matcher.Instruction.IsStarg())
            matcher.SetInstructionAndAdvance(Code.Nop);
        matcher.SetInstructionAndAdvance(Code.Nop);

        matcher.MatchStartForward(CodeMatch.Calls(checkAllBoolMethod));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.Calls(checkAllBoolMethod));
        matcher.SetInstruction(Transpilers.EmitDelegate(CheckAllBoolUsingListValuesInput));
        matcher.MatchStartBackwards(CodeMatch.LoadsArgument());
        matcher.SetInstructionAndAdvance(CodeInstruction.LoadLocal(localIndex));

        return matcher.Instructions();
    }

    private static List<int> GetClearedPreallocatedList()
    {
        TemporaryLimits.Clear();
        return TemporaryLimits;
    }

    private static bool CheckAllBoolUsingListValuesInput(bool[] array, List<int> values, bool state)
    {
        foreach (int value in values)
        {
            if (array[value] != state)
                return false;
        }

        return true;
    }
}