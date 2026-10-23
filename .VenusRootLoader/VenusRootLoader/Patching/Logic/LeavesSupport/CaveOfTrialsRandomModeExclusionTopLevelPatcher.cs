using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher adds support for allowing <see cref="EnemyLeaf"/> to decide if they should be included or excluded from
/// Cave Of Trials random mode pool.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="EventControl.GetRandomEnemy"/>: Replaces a while loop condition to only reject enemy game ids that
/// the registry says to exclude.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class CaveOfTrialsRandomModeExclusionTopLevelPatcher : ITopLevelPatcher
{
    private static CaveOfTrialsRandomModeExclusionTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    public CaveOfTrialsRandomModeExclusionTopLevelPatcher(ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _instance = this;
        _enemiesRegistry = enemiesRegistry;
    }

    public void Patch()
    {
        Md.EventControl.GetRandomEnemy.ILHook(ChangeCaveOfTrialsRandomModeExclusions);
    }

    private static void ChangeCaveOfTrialsRandomModeExclusions(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        ILLabel startLoopLabel = null!;
        cursor.GotoNext(x => x.MatchBranch(out startLoopLabel!));
        int returnValueLocalIndex = 0;
        cursor.GotoPrev(x => x.MatchLdloc(out returnValueLocalIndex));
        cursor.GotoPrev(x => x.MatchStloc(out _));
        cursor.Index++;

        // We are effectively hijacking the while condition here by going to the loop start if it's excluded and returning
        // immediately if it's included.
        cursor.EmitLdloc(returnValueLocalIndex);
        cursor.EmitDelegate(IsAllowedOnCaveOfTrialsRandomMode);
        cursor.EmitBrfalse(startLoopLabel);
        cursor.EmitLdloc(returnValueLocalIndex);
        cursor.EmitRet();
    }

    private static bool IsAllowedOnCaveOfTrialsRandomMode(int enemyId) =>
        _instance._enemiesRegistry.GetByGameId(enemyId).IsIncludedInRandomCaveOfTrialsPool;
}