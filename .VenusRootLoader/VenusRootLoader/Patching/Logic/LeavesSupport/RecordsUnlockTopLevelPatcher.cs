using InputIOManager;
using MonoDetour.DetourTypes;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows custom <see cref="RecordLeaf"/> to be unlocked without the game attempting to unlock the platform
/// specific achievements associated with them. This is meant to prevent Steam and GOG achievements to be unlocked for
/// custom records which cannot have achievements backing them.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="InputIO.Achivement"/>: Prevents the original method to run if the game id isn't part of the base game.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class RecordsUnlockTopLevelPatcher : ITopLevelPatcher
{
    private static RecordsUnlockTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<RecordLeaf> _orderedEnemiesRegistry;

    public RecordsUnlockTopLevelPatcher(ILeavesRegistry<RecordLeaf> orderedEnemiesRegistry)
    {
        _instance = this;
        _orderedEnemiesRegistry = orderedEnemiesRegistry;
    }

    public void Patch()
    {
        Md.InputIOManager.InputIO.Achivement.ControlFlowPrefix(AllowPlatformSpecificUnlock);
    }

    private static ReturnFlow AllowPlatformSpecificUnlock(ref int id) =>
        id < _instance._orderedEnemiesRegistry.CountBaseGame ? ReturnFlow.None : ReturnFlow.SkipOriginal;
}