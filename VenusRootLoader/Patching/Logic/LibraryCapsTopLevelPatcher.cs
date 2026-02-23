using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class LibraryCapsTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesRegistry;

    private static LibraryCapsTopLevelPatcher _instance = null!;

    public LibraryCapsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<DiscoveryLeaf> discoveriesRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _discoveriesRegistry = discoveriesRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(LibraryCapsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.Load))]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.SetVariables))]
    internal static IEnumerable<CodeInstruction> RemoveFlagsHardCapSetVariables(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        bool isSetVariables = method.Name == nameof(MainManager.SetVariables);
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo libraryField = AccessTools.Field(
            typeof(MainManager),
            isSetVariables
                ? nameof(MainManager.librarydata)
                : nameof(MainManager.librarystuff));

        matcher.MatchStartForward(CodeMatch.StoresField(libraryField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        if (isSetVariables)
        {
            matcher.Advance(-1);
            matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        }

        matcher.Advance(1);
        matcher.Insert(Transpilers.EmitDelegate(GetNewLibraryStuffCap));

        return matcher.Instructions();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainManager), MethodType.Constructor)]
    internal static bool PatchLibraryLimit()
    {
        MainManager.librarylimit[0] = _instance._discoveriesRegistry.LeavesByNamedIds.Count;
        return true;
    }

    private static int GetNewLibraryStuffCap(int baseGameCap)
    {
        int newCap = _instance._discoveriesRegistry.LeavesByNamedIds.Count;
        return baseGameCap < newCap ? newCap : baseGameCap;
    }
}