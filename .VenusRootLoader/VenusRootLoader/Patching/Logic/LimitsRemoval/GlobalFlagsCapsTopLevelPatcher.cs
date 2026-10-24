using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

/// <summary>
/// This patcher adds support for a variable amount of <see cref="FlagLeaf"/>, <see cref="FlagvarLeaf"/> and
/// <see cref="FlagstringLeaf"/> to exist in the game.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetVariables"/>: Changes the hardcoded length of <see cref="MainManager.flags"/>,
/// <see cref="MainManager.flagvar"/> and <see cref="MainManager.flagstring"/>.</item>
/// <item><see cref="MainManager.Load"/>: Changes the hardcoded length of <see cref="MainManager.flags"/>,
/// <see cref="MainManager.flagvar"/> and <see cref="MainManager.flagstring"/>.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class GlobalFlagsCapsTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<FlagLeaf> _flagsLeafRegistry;
    private readonly ILeavesRegistry<FlagvarLeaf> _flagvarsLeafRegistry;
    private readonly ILeavesRegistry<FlagstringLeaf> _flagstringsLeafRegistry;

    private static GlobalFlagsCapsTopLevelPatcher _instance = null!;

    public GlobalFlagsCapsTopLevelPatcher(
        ILeavesRegistry<FlagLeaf> flagsLeafRegistry,
        ILeavesRegistry<FlagvarLeaf> flagvarsLeafRegistry,
        ILeavesRegistry<FlagstringLeaf> flagstringsLeafRegistry)
    {
        _instance = this;
        _flagsLeafRegistry = flagsLeafRegistry;
        _flagvarsLeafRegistry = flagvarsLeafRegistry;
        _flagstringsLeafRegistry = flagstringsLeafRegistry;
    }

    public void Patch()
    {
        Md.MainManager.SetVariables.ILHook(RemoveFlagsHardCap);
        Md.MainManager.Load.ILHook(RemoveFlagsHardCap);
    }

    private static void RemoveFlagsHardCap(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        // We do this patch on Load because not doing it will cause Load to undo the SetVariables patch by overwritting the array.
        // This is still fine to do because while it technically changes the amount the game will attempt to read from the save,
        // it won't consider any indexes past the amount present on the save itself. This will leave all leftovers values
        // to false which is what we want.
        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.flags)));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.SetToStaticDelegateCallAndAdvance(GetNewFlagsCap);
        cursor.Goto(0);
        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.flagvar)));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.SetToStaticDelegateCallAndAdvance(GetNewFlagvarsCap);
        cursor.Goto(0);
        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.flagstring)));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.SetToStaticDelegateCall(GetNewFlagstringsCap);
    }

    private static int GetNewFlagsCap() => _instance._flagsLeafRegistry.Count;
    private static int GetNewFlagvarsCap() => _instance._flagvarsLeafRegistry.Count;
    private static int GetNewFlagstringsCap() => _instance._flagstringsLeafRegistry.Count;
}