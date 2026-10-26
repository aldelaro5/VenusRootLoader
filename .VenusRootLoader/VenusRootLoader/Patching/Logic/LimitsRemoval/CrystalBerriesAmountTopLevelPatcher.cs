using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

/// <summary>
/// This patcher adds support for a variable amount of <see cref="CrystalBerryLeaf"/> to exist in the game.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetVariables"/>: Changes the hardcoded length of <see cref="MainManager.crystalbflags"/>.</item>
/// <item><see cref="MainManager.Load"/>: Changes the length of <see cref="MainManager.crystalbflags"/> such that it
/// picks the length of the registry if it exceeds the one read on the save file.</item>
/// <item><see cref="MainManager.CheckAchievement"/>: Changes the amount of needed <see cref="MainManager.crystalbflags"/>
/// to be true for the Crystal Berries record to be unlocked.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class CrystalBerriesAmountTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<CrystalBerryLeaf> _crystalBerriesLeafRegistry;

    private static CrystalBerriesAmountTopLevelPatcher _instance = null!;

    public CrystalBerriesAmountTopLevelPatcher(ILeavesRegistry<CrystalBerryLeaf> crystalBerriesLeafRegistry)
    {
        _instance = this;
        _crystalBerriesLeafRegistry = crystalBerriesLeafRegistry;
    }

    public void Patch()
    {
        Md.MainManager.SetVariables.ILHook(RemoveCrystalBerriesHardCap);
        Md.MainManager.Load.ILHook(AdjustCrystalBerriesAmountFromSave);
        Md.MainManager.CheckAchievement_Predicate_1.ILHook(AdjustCrystalBerriesRecordCheck);
    }

    private static void RemoveCrystalBerriesHardCap(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.crystalbflags)));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.SetToStaticDelegateCall(GetNewCrystalBerriesCap);
    }

    private static void AdjustCrystalBerriesAmountFromSave(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        // The save loading process will actually attempt to overwrite the entire array with the lengths it got from the save.
        // We don't want this because it would undo our SetVariables patch so we want to patch this length. What's interesting
        // is this will not break save loading because Load will still use the length found in the actual save to read it.
        // What's actually going to happen is any leftover data will be left to default values, which is what we want.
        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.crystalbflags)));
        cursor.GotoPrev(x => x.MatchNewarr(out _));
        cursor.EmitDelegate(GetBestCrystalBerriesAmountFromSave);
    }

    private static void AdjustCrystalBerriesRecordCheck(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchCallOrCallvirt<MainManager>(nameof(MainManager.CrystalBerryAmmount)));
        cursor.GotoNext(x => x.MatchLdcI4(out _));
        cursor.SetToStaticDelegateCall(GetNewCrystalBerriesCap);
    }

    private static int GetNewCrystalBerriesCap() => _instance._crystalBerriesLeafRegistry.Count;

    // The reason it needs to be the Math.Max between the 2 is because it's possible the save doesn't contain all the
    // Crystal Berries in the registry. In that case, we want to load the amount of the save, but the game will still
    // use the length from our registry and leave the values to false which is what we want.
    private static int GetBestCrystalBerriesAmountFromSave(int amountFromSaveFile) =>
        Math.Max(amountFromSaveFile, _instance._crystalBerriesLeafRegistry.Count);
}