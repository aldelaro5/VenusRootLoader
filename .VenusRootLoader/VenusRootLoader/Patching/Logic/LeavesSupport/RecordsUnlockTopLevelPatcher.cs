using InputIOManager;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows custom <see cref="RecordLeaf"/> to be automatically unlocked with custom predicates. It also
/// allows them to be unlocked without the game attempting to unlock the platform specific achievements associated with
/// them. This is meant to prevent Steam and GOG achievements to be unlocked for custom records which cannot have
/// achievements backing them.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="InputIO.Achivement"/>: Prevents the original method to run if the game id isn't part of the base game.</item>
/// <item><see cref="MainManager.CheckAchievement"/>: Enforce the auto unlock conditions to be sourced from the
/// <see cref="RecordLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class RecordsUnlockTopLevelPatcher : ITopLevelPatcher
{
    private const int CompletionRecordGameId = 27;
    private static RecordsUnlockTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<RecordLeaf> _orderedRecordsRegistry;

    public RecordsUnlockTopLevelPatcher(ILeavesRegistry<RecordLeaf> orderedRecordsRegistry)
    {
        _instance = this;
        _orderedRecordsRegistry = orderedRecordsRegistry;
    }

    public void Patch()
    {
        Md.InputIOManager.InputIO.Achivement.ControlFlowPrefix(AllowPlatformSpecificUnlock);
        Md.MainManager.CheckAchievement.ILHook(PatchCheckAchievement);
    }

    private static ReturnFlow AllowPlatformSpecificUnlock(ref int id) =>
        id < _instance._orderedRecordsRegistry.CountBaseGame ? ReturnFlow.None : ReturnFlow.SkipOriginal;

    private static void PatchCheckAchievement(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchBranch(out _));
        cursor.Index++;
        int indexPatch = cursor.Index;

        // The last UpdateJounal call unlocks a discovery which we don't want to bypass so we need to look for the second
        // to last call.
        cursor.Goto(-1);
        cursor.GotoPrev(x => x.MatchCallOrCallvirt<MainManager>(nameof(MainManager.UpdateJounal)));
        cursor.Index--;
        cursor.GotoPrev(x => x.MatchCallOrCallvirt<MainManager>(nameof(MainManager.UpdateJounal)));
        cursor.Index++;
        ILLabel labelResume = cursor.MarkLabel();

        cursor.Goto(indexPatch);
        cursor.EmitLdarg0();
        cursor.EmitDelegate(CheckAchievementHook);
        cursor.EmitBr(labelResume);
    }

    private static void CheckAchievementHook(MainManager instance)
    {
        foreach (RecordLeaf recordLeaf in _instance._orderedRecordsRegistry)
        {
            // The base game does this instead of enforcing that the record was unlocked already.
            if (recordLeaf.GameId == CompletionRecordGameId && instance.flags[63])
                continue;

            if (instance.librarystuff[(int)MainManager.LibraryPages.Logbook, recordLeaf.GameId])
                continue;

            if (recordLeaf.AutoUnlockConditions?.Invoke(instance) ?? false)
            {
                MainManager.UpdateJounal(MainManager.Library.Logbook, recordLeaf.GameId);
                if (recordLeaf.GameId == CompletionRecordGameId)
                    instance.flags[63] = true;
            }
        }
    }
}