using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

/// <summary>
/// This patcher adds support for a variable amount of <see cref="EnemyLeaf"/> to exist in the game by patching the amount
/// of allowed <see cref="MainManager.enemyencounter"/> that is used so it can go beyond 256.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetVariables"/>: Changes the hardcoded length 0 of <see cref="MainManager.enemyencounter"/>
/// so it picks the one from the registry if it exceeds the base game one.</item>
/// <item><see cref="MainManager.Load"/>: Changes the length 0 of <see cref="MainManager.enemyencounter"/> such that it
/// picks the length of the registry if it exceeds the one read on the save file.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class EnemyEncounterCapTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    private static EnemyEncounterCapTopLevelPatcher _instance = null!;

    public EnemyEncounterCapTopLevelPatcher(ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _instance = this;
        _enemiesRegistry = enemiesRegistry;
    }

    public void Patch()
    {
        Md.MainManager.SetVariables.ILHook(RemoveEnemyEncounterHardCap);
        Md.MainManager.Load.ILHook(AdjustEnemyEncounterAmountFromSave);
    }

    private static void RemoveEnemyEncounterHardCap(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.enemyencounter)));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.EmitDelegate(GetNewEnemyEncounterCap);
    }

    private static void AdjustEnemyEncounterAmountFromSave(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        // The save loading process will actually attempt to overwrite the entire array with the lengths it got from the save.
        // We don't want this because it would undo our SetVariables patch so we want to patch this length. What's interesting
        // is this will not break save loading because Load will still use the length found in the actual save to read it.
        // What's actually going to happen is any leftover data will be left to default values, which is what we want.
        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.enemyencounter)));
        // This is a 2D array so we need to seek ldlen twice.
        cursor.GotoPrev(x => x.MatchLdlen());
        cursor.Index--;
        cursor.GotoPrev(x => x.MatchLdlen());
        cursor.GotoNext(x => x.MatchConvI4());
        cursor.GotoNext(x => x.MatchLdloc(out _));
        cursor.EmitDelegate(GetBestEnemyEncounterAmountFromSave);
    }

    private static int GetNewEnemyEncounterCap(int originalCap) =>
        Math.Max(originalCap, _instance._enemiesRegistry.Count);

    // The reason it needs to be the Math.Max between the 2 is because it's possible the save doesn't contain all the
    // enemy encounters in the registry. In that case, we want to load the amount of the save, but the game will still
    // use the length from our registry and leave the values to 0 seen, 0 defeated which is what we want.
    private static int GetBestEnemyEncounterAmountFromSave(int amountFromSaveFile) =>
        Math.Max(amountFromSaveFile, _instance._enemiesRegistry.Count);
}