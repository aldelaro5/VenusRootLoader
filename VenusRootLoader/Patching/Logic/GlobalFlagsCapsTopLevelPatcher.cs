using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class GlobalFlagsCapsTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<FlagLeaf> _flagsLeafRegistry;
    private readonly ILeavesRegistry<FlagvarLeaf> _flagvarsLeafRegistry;
    private readonly ILeavesRegistry<FlagstringLeaf> _flagstringsLeafRegistry;

    private static GlobalFlagsCapsTopLevelPatcher _instance = null!;

    public GlobalFlagsCapsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<FlagLeaf> flagsLeafRegistry,
        ILeavesRegistry<FlagvarLeaf> flagvarsLeafRegistry,
        ILeavesRegistry<FlagstringLeaf> flagstringsLeafRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _flagsLeafRegistry = flagsLeafRegistry;
        _flagvarsLeafRegistry = flagvarsLeafRegistry;
        _flagstringsLeafRegistry = flagstringsLeafRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(GlobalFlagsCapsTopLevelPatcher));

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
        FieldInfo flagstringField = AccessTools.Field(typeof(MainManager), nameof(MainManager.flagstring));

        matcher.MatchStartForward(CodeMatch.StoresField(flagField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(GetNewFlagsCap));
        matcher.Start();
        matcher.MatchStartForward(CodeMatch.StoresField(flagvarField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(GetNewFlagvarsCap));
        matcher.Start();
        matcher.MatchStartForward(CodeMatch.StoresField(flagstringField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(GetNewFlagstringsCap));

        return matcher.Instructions();
    }

    private static int GetNewFlagsCap() => _instance._flagsLeafRegistry.Leaves.Count;
    private static int GetNewFlagvarsCap() => _instance._flagvarsLeafRegistry.Leaves.Count;
    private static int GetNewFlagstringsCap() => _instance._flagstringsLeafRegistry.Leaves.Count;
}