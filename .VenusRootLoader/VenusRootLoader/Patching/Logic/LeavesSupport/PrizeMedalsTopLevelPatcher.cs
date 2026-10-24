using MonoDetour.Cil;
using MonoMod.Cil;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher adds support for <see cref="PrizeMedalLeaf"/> since their data are normally hardcoded using the following arrays:
/// <list type="bullet">
/// <item><see cref="MainManager.prizeids"/>: The medal game ids.</item>
/// <item><see cref="MainManager.prizeflags"/>: The bound flagvar game ids.</item>
/// <item><see cref="MainManager.prizeenemyids"/>: The displayed enemy game ids.</item>
/// </list>
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetVariables"/>: Changes the 3 arrays to reflect the state of the prize medal registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class PrizeMedalsTopLevelPatcher : ITopLevelPatcher
{
    private static PrizeMedalsTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<PrizeMedalLeaf> _prizeMedalsRegistry;

    public PrizeMedalsTopLevelPatcher(ILeavesRegistry<PrizeMedalLeaf> prizeMedalsRegistry)
    {
        _instance = this;
        _prizeMedalsRegistry = prizeMedalsRegistry;
    }

    public void Patch()
    {
        Md.MainManager.SetVariables.ILHook(PatchPrizeMedalsData);
    }

    private static void PatchPrizeMedalsData(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        FieldInfo prizeIdsField = typeof(MainManager).FieldByName(nameof(MainManager.prizeids))!;
        PatchPrizeMedalArray(cursor, prizeIdsField, GetPrizeIds);
        FieldInfo prizeFlagsField = typeof(MainManager).FieldByName(nameof(MainManager.prizeflags))!;
        PatchPrizeMedalArray(cursor, prizeFlagsField, GetPrizeFlagIds);
        FieldInfo prizeEnemyIdsField = typeof(MainManager).FieldByName(nameof(MainManager.prizeenemyids))!;
        PatchPrizeMedalArray(cursor, prizeEnemyIdsField, GetPrizeEnemyIds);
    }

    // We are essentially NOPing the creation of the array and replace it with an array we control.
    private static void PatchPrizeMedalArray(
        ILCursor cursor,
        FieldInfo arrayField,
        Func<int[]> arrayPatcher)
    {
        cursor.GotoNext(x => x.MatchStfld(arrayField));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        while (!cursor.Current.MatchStfld(arrayField))
            cursor.SetAndAdvance(OpCodes.Nop);
        cursor.Index--;
        cursor.SetToStaticDelegateCall(arrayPatcher);
        cursor.Goto(0);
    }

    private static int[] GetPrizeIds() =>
        _instance._prizeMedalsRegistry.Select(l => l.Medal.Resolve().GameId).ToArray();

    private static int[] GetPrizeFlagIds() =>
        _instance._prizeMedalsRegistry.Select(l => l.Flagvar.Resolve().GameId).ToArray();

    private static int[] GetPrizeEnemyIds() =>
        _instance._prizeMedalsRegistry.Select(l => l.DisplayedEnemyGameId).ToArray();
}