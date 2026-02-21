using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class GlobalFlagsCapsPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<FlagLeaf> _flagsLeafRegistry;
    private readonly ILeavesRegistry<FlagvarLeaf> _flagvarsLeafRegistry;

    private static GlobalFlagsCapsPatcher _instance = null!;

    public GlobalFlagsCapsPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<FlagLeaf> flagsLeafRegistry,
        ILeavesRegistry<FlagvarLeaf> flagvarsLeafRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _flagsLeafRegistry = flagsLeafRegistry;
        _flagvarsLeafRegistry = flagvarsLeafRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(GlobalFlagsCapsPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.SetVariables))]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.Load))]
    internal static IEnumerable<CodeInstruction> RemoveFlagsHardCap(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo flagField = AccessTools.Field(typeof(MainManager), nameof(MainManager.flags));
        FieldInfo flagvarField = AccessTools.Field(typeof(MainManager), nameof(MainManager.flagvar));

        matcher.MatchStartForward(CodeMatch.StoresField(flagField));
        matcher.MatchStartBackwards(Code.Ldc_I4);
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(GetNewFlagsCap));
        matcher.Start();
        matcher.MatchStartForward(CodeMatch.StoresField(flagvarField));
        matcher.MatchStartBackwards(Code.Ldc_I4);
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(GetNewFlagvarsCap));

        return matcher.Instructions();
    }

    private static int GetNewFlagsCap() => _instance._flagsLeafRegistry.Leaves.Count;
    private static int GetNewFlagvarsCap() => _instance._flagvarsLeafRegistry.Leaves.Count;
}