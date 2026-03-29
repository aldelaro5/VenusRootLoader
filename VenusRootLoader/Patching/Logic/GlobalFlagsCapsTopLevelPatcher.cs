using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for a variable amount of <see cref="FlagLeaf"/>, <see cref="FlagvarLeaf"/> and <see cref="FlagstringLeaf"/> to exist in the game.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetVariables"/>: Changes the hardcoded length of <see cref="MainManager.flags"/>, <see cref="MainManager.flagvar"/> and <see cref="MainManager.flagstring"/>.</item>
/// <item><see cref="MainManager.Load"/>: Changes the hardcoded length of <see cref="MainManager.flags"/>, <see cref="MainManager.flagvar"/> and <see cref="MainManager.flagstring"/>.</item>
/// </list>
/// </p>
/// </summary>
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

        // We do this patch on Load because not doing it will cause Load to undo the SetVariables patch by overwritting the array.
        // This is still fine to do because while it technically changes the amount the game will attempt to read from the save,
        // it won't consider any indexes past the amount present on the save itself. This will leave all leftovers values
        // to false which is what we want.
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

    private static int GetNewFlagsCap() => _instance._flagsLeafRegistry.LeavesByNamedIds.Count;
    private static int GetNewFlagvarsCap() => _instance._flagvarsLeafRegistry.LeavesByNamedIds.Count;
    private static int GetNewFlagstringsCap() => _instance._flagstringsLeafRegistry.LeavesByNamedIds.Count;
}