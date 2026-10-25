using MonoDetour.Cil;
using MonoMod.Cil;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

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
/// <item><see cref="MainManager.SetVariables"/>: Changes the hardcoded length 1 of <see cref="MainManager.librarydata"/>
/// to be the largest number needed to accomodate all 5 registries.</item>
/// <item><see cref="MainManager.Load"/>: Changes the hardcoded length 1 of <see cref="MainManager.librarystuff"/> to be
/// the largest number needed to accomodate all 5 registries.</item>
/// <item><see cref="MainManager()"/>: Changes the hardcoded values of <see cref="MainManager.librarylimit"/> to match
/// the amounts in the 5 registries.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class LibraryCapsTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesRegistry;
    private readonly IOrderedLeavesRegistry<EnemyLeaf> _orderedEnemiesRegistry;
    private readonly ILeavesRegistry<RecipeLibraryEntryLeaf> _recipeLibraryEntriesRegistry;
    private readonly ILeavesRegistry<RecordLeaf> _recordsRegistry;
    private readonly ILeavesRegistry<AreaLeaf> _areasRegistry;

    private static LibraryCapsTopLevelPatcher _instance = null!;

    public LibraryCapsTopLevelPatcher(
        ILeavesRegistry<DiscoveryLeaf> discoveriesRegistry,
        IOrderedLeavesRegistry<EnemyLeaf> orderedEnemiesRegistry,
        ILeavesRegistry<RecipeLibraryEntryLeaf> recipeLibraryEntriesRegistry,
        ILeavesRegistry<RecordLeaf> recordsRegistry,
        ILeavesRegistry<AreaLeaf> areasRegistry)
    {
        _instance = this;
        _discoveriesRegistry = discoveriesRegistry;
        _orderedEnemiesRegistry = orderedEnemiesRegistry;
        _recipeLibraryEntriesRegistry = recipeLibraryEntriesRegistry;
        _recordsRegistry = recordsRegistry;
        _areasRegistry = areasRegistry;
    }

    public void Patch()
    {
        Md.MainManager.SetVariables.ILHook(RemoveFlagsHardCapSetVariables);
        Md.MainManager.Load.ILHook(RemoveFlagsHardCapSetVariables);
        Md.MainManager._ctor.Prefix(PatchLibraryLimit);
    }

    // The save loading process will actually attempt to overwrite the entire array with the lengths it got from the save.
    // We don't want this because it would undo our SetVariables patch so we want to patch this length. What's interesting
    // is this will not break save loading because Load will still use the length found in the actual save to read it.
    // What's actually going to happen is any leftover data will be left to default values, which is what we want.
    private static void RemoveFlagsHardCapSetVariables(ILManipulationInfo info)
    {
        bool isSetVariables = info.Original!.Name == nameof(MainManager.SetVariables);
        ILCursor cursor = new(info.Context);
        FieldInfo libraryField = typeof(MainManager).FieldByName(
            isSetVariables
                ? nameof(MainManager.librarydata)
                : nameof(MainManager.librarystuff))!;

        cursor.GotoNext(x => isSetVariables
            ? x.MatchStsfld(libraryField)
            : x.MatchStfld(libraryField));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        // Both of them are multidimensional arrays, but the length we want to change for librarydata is the second to last one
        // while it's the last one for librarystuff which explains why we need to seek twice on the former.
        if (isSetVariables)
        {
            cursor.Index--;
            cursor.GotoNext(x => x.MatchLdcI4(out _));
        }

        cursor.Index++;
        cursor.EmitDelegate(GetNewLibraryStuffCap);
    }

    private static void PatchLibraryLimit(MainManager instance)
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
    }

    private static int GetNewLibraryStuffCap(int baseGameCap)
    {
        int newCap = Enumerable.Max(
        [
            _instance._discoveriesRegistry.Count,
            _instance._orderedEnemiesRegistry.Registry.Count,
            _instance._recipeLibraryEntriesRegistry.Count,
            _instance._recordsRegistry.Count,
            _instance._areasRegistry.Count
        ]);
        return baseGameCap < newCap ? newCap : baseGameCap;
    }

    private static void AssignNewLibraryLimitByPage<T>(
        MainManager.Library page,
        ILeavesRegistry<T> registry,
        int[] original)
        where T : Leaf
    {
        MainManager.librarylimit[(int)page] = Math.Max(registry.Count, original[(int)page]);
    }
}