using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for a variable amount of library related registries to exist in the game.
/// Those registries impact the following leaves:
/// <list type="bullet">
/// <item><see cref="DiscoveryLeaf"/></item>
/// <item><see cref="EnemyLeaf"/></item>
/// <item><see cref="RecipeLibraryEntryLeaf"/></item>
/// <item><see cref="RecordLeaf"/></item>
/// <item><see cref="AreaLeaf"/></item>
/// </list>
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetVariables"/>: Changes the hardcoded length 1 of <see cref="MainManager.librarydata"/> to be the largest number needed to accomodate all 5 registries.</item>
/// <item><see cref="MainManager.Load"/>: Changes the hardcoded length 1 of <see cref="MainManager.librarystuff"/> to be the largest number needed to accomodate all 5 registries.</item>
/// <item><see cref="MainManager()"/>: Changes the hardcoded values of <see cref="MainManager.librarylimit"/> to match the amounts in the 5 registries.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class LibraryCapsTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesRegistry;
    private readonly IOrderedLeavesRegistry<EnemyLeaf> _orderedEnemiesRegistry;
    private readonly ILeavesRegistry<RecipeLibraryEntryLeaf> _recipeLibraryEntriesRegistry;
    private readonly ILeavesRegistry<RecordLeaf> _recordsRegistry;
    private readonly ILeavesRegistry<AreaLeaf> _areasRegistry;

    private static LibraryCapsTopLevelPatcher _instance = null!;

    public LibraryCapsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<DiscoveryLeaf> discoveriesRegistry,
        IOrderedLeavesRegistry<EnemyLeaf> orderedEnemiesRegistry,
        ILeavesRegistry<RecipeLibraryEntryLeaf> recipeLibraryEntriesRegistry,
        ILeavesRegistry<RecordLeaf> recordsRegistry,
        ILeavesRegistry<AreaLeaf> areasRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _discoveriesRegistry = discoveriesRegistry;
        _orderedEnemiesRegistry = orderedEnemiesRegistry;
        _recipeLibraryEntriesRegistry = recipeLibraryEntriesRegistry;
        _recordsRegistry = recordsRegistry;
        _areasRegistry = areasRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(LibraryCapsTopLevelPatcher));

    // The save loading process will actually attempt to overwrite the entire array with the lengths it got from the save.
    // We don't want this because it would undo our SetVariables patch so we want to patch this length. What's interesting
    // is this will not break save loading because Load will still use the length found in the actual save to read it.
    // What's actually going to happen is any leftover data will be left to default values, which is what we want.
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
        // Both of them are multidimensional arrays, but the length we want to change for librarydata is the second to last one
        // while it's the last one for librarystuff which explains why we need to seek twice on the former.
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
        int[] original = new int[Enum.GetValues(typeof(MainManager.Library)).Length];
        MainManager.librarylimit.CopyTo(original);

        // While the length of librarydata and librarystuff are overprovisioned normally, librarylimit tells the ACTUAL
        // used lengths of the arrays so we need to patch them to the length of our registries.
        AssignNewLibraryLimitByPage(MainManager.Library.Discovery, _instance._discoveriesRegistry, original);
        int amountEnemiesInBestiary = _instance._orderedEnemiesRegistry.GetOrderedLeaves().Count;
        MainManager.librarylimit[(int)MainManager.LibraryPages.Bestiary] = Math.Max(
            amountEnemiesInBestiary,
            original[(int)MainManager.LibraryPages.Bestiary]);
        AssignNewLibraryLimitByPage(MainManager.Library.Recipes, _instance._recipeLibraryEntriesRegistry, original);
        AssignNewLibraryLimitByPage(MainManager.Library.Logbook, _instance._recordsRegistry, original);
        AssignNewLibraryLimitByPage(MainManager.Library.Map, _instance._areasRegistry, original);

        return true;
    }

    private static int GetNewLibraryStuffCap(int baseGameCap)
    {
        int newCap = Enumerable.Max(
        [
            _instance._discoveriesRegistry.LeavesByNamedIds.Count,
            _instance._orderedEnemiesRegistry.Registry.LeavesByNamedIds.Count,
            _instance._recipeLibraryEntriesRegistry.LeavesByNamedIds.Count,
            _instance._recordsRegistry.LeavesByNamedIds.Count,
            _instance._areasRegistry.LeavesByNamedIds.Count
        ]);
        return baseGameCap < newCap ? newCap : baseGameCap;
    }

    private static void AssignNewLibraryLimitByPage<T>(
        MainManager.Library page,
        ILeavesRegistry<T> registry,
        int[] original)
        where T : Leaf
    {
        MainManager.librarylimit[(int)page] = Math.Max(registry.LeavesByNamedIds.Count, original[(int)page]);
    }
}