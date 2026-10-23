using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Extensions;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace VenusRootLoader.Patching.Logic.BaseGameFixes;

/// <summary>
/// This patcher improves the memory allocation performance of the game by changing <see cref="MainManager.CheckIfCanExist"/>
/// so it doesn't allocate any memory, and it also changes <see cref="MainManager.DoClock"/> to not prematurely cause Garbage collections.
/// These changes were shown using the Unity profiler to be very effective at reducing stutters which would get worse in <see cref="VenusRootLoader"/>
/// because of the increased complexity of a <see cref="GC.Collect()"/> call.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.CheckIfCanExist"/>: Fixes an unnecessary heap allocation.</item>
/// <item><see cref="MainManager.DoClock"/>: Removes a forced GC collection which becomes less useful due to the above fix.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class MemoryAllocationsTopLevelPatcher : ITopLevelPatcher
{
    private static readonly List<int> TemporaryLimits = new();

    public void Patch()
    {
        Md.MainManager.DoClock.ILHook(RemoveForcedGcCollection);
        Md.MainManager.CheckIfCanExist.ILHook(FixMemoryAllocation);
    }

    private static void RemoveForcedGcCollection(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.clocksec)));
        cursor.Index++;
        while (!cursor.Current.MatchCallOrCallvirt(typeof(GC), nameof(GC.Collect)))
            cursor.SetAndAdvance(OpCodes.Nop);
        cursor.SetAndAdvance(OpCodes.Nop);
    }

    private static void FixMemoryAllocation(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchNewobj(out _));
        cursor.SetToStaticDelegateCallAndAdvance(GetClearedPreallocatedList);
        cursor.Current.MatchLdloc(out int localIndex);
        cursor.GotoNext(x => x.MatchBranch(out _));
        ILLabel labelForConditionCheck = (ILLabel)cursor.Current.Operand;
        cursor.GotoLabel(labelForConditionCheck);
        cursor.GotoNext(x => x.MatchLdloc(localIndex));
        while (!cursor.Current.MatchStarg(out _))
            cursor.SetAndAdvance(OpCodes.Nop);
        cursor.SetAndAdvance(OpCodes.Nop);

        cursor.GotoNext(x => x.MatchCallOrCallvirt<MainManager>(nameof(MainManager.CheckAllBool)));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchCallOrCallvirt<MainManager>(nameof(MainManager.CheckAllBool)));
        cursor.SetToStaticDelegateCall(CheckAllBoolUsingListValuesInput);
        cursor.GotoPrev(x => x.MatchLdarg(out _));
        cursor.SetAndAdvance(OpCodes.Ldloc, localIndex);
    }

    private static List<int> GetClearedPreallocatedList()
    {
        TemporaryLimits.Clear();
        return TemporaryLimits;
    }

    private static bool CheckAllBoolUsingListValuesInput(bool[] array, List<int> values, bool state)
    {
        foreach (int value in values)
        {
            if (array[value] != state)
                return false;
        }

        return true;
    }
}