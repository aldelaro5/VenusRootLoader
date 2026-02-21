using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class FlagsCapsPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<FlagLeaf> _flagsLeafRegistry;

    private static FlagsCapsPatcher _instance = null!;

    public FlagsCapsPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<FlagLeaf> flagsLeafRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _flagsLeafRegistry = flagsLeafRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(FlagsCapsPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.SetVariables))]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.Load))]
    internal static IEnumerable<CodeInstruction> RemoveFlagsHardCap(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo flagField = AccessTools.Field(typeof(MainManager), nameof(MainManager.flags));

        matcher.MatchStartForward(CodeMatch.StoresField(flagField));
        matcher.MatchStartBackwards(Code.Ldc_I4);
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(GetNewFlagsCap));

        return matcher.Instructions();
    }

    private static int GetNewFlagsCap() => _instance._flagsLeafRegistry.Leaves.Count;
}