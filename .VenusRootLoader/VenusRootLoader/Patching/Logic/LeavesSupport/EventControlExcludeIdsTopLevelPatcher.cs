using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="EnemyLeaf"/> registry to reflect which one is excluded or included in the
/// bestiary when speaking to Tattl (event 65).
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="EventControl()"/>: Changes <see cref="EventControl.excludeids"/> to reflect information from the
/// <see cref="EnemyLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class EventControlExcludeIdsTopLevelPatcher : ITopLevelPatcher
{
    private static EventControlExcludeIdsTopLevelPatcher _instance = null!;

    private readonly IOrderedLeavesRegistry<EnemyLeaf> _orderedEnemiesRegistry;

    public EventControlExcludeIdsTopLevelPatcher(IOrderedLeavesRegistry<EnemyLeaf> orderedEnemiesRegistry)
    {
        _instance = this;
        _orderedEnemiesRegistry = orderedEnemiesRegistry;
    }

    public void Patch()
    {
        Md.EventControl._ctor.ILHook(AdjustExcludeIds);
    }

    private static void AdjustExcludeIds(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchStfld<EventControl>(nameof(EventControl.excludeids)));
        cursor.GotoPrev(x => x.MatchNewobj(out _));
        cursor.EmitDelegate(GetNewExcludeIds);
    }

    private static IEnumerable<int> GetNewExcludeIds(IEnumerable<int> original)
    {
        ILeavesRegistry<EnemyLeaf> allEnemies = _instance._orderedEnemiesRegistry.Registry;
        List<EnemyLeaf> enemiesInBestiary = _instance._orderedEnemiesRegistry.GetOrderedLeaves().ToList();

        return allEnemies
            .Except(enemiesInBestiary)
            .Select(l => l.GameId);
    }
}