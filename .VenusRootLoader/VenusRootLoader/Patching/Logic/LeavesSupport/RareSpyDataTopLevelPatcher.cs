using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher adds support for allowing <see cref="EnemyLeaf"/> to decide if they should be considered rare Spy Data
/// when talking to Tattl.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="EventControl.Event65"/>: Changes the list of rare Spy Data enemy game ids to reflect the registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class RareSpyDataTopLevelPatcher : ITopLevelPatcher
{
    private static RareSpyDataTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    public RareSpyDataTopLevelPatcher(ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _instance = this;
        _enemiesRegistry = enemiesRegistry;
    }

    public void Patch()
    {
        Md.EventControl.Event65.ILHookMoveNext(ChangeRareSpyDataList);
    }

    private static void ChangeRareSpyDataList(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        // What we are essentially doing here is look for when the game adds the hardcoded GoldenSeedling to the list,
        // but then overwrite that logic with a delegate that clears the entire list and adds our own elements.
        // We do this because it's the last base game manipulation the game does, and it has to do this because the GoldenSeedling
        // isn't a boss / miniboss so we can sort of rely on this being there.
        cursor.GotoNext(x => x.MatchCallOrCallvirt<List<int>>(nameof(List<>.Add)));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.EmitDelegate(ChangeRareSpyDataEnemies);
        while (!cursor.Instrs[cursor.Index].MatchLdarg0())
            cursor.SetAndAdvance(OpCodes.Nop);
    }

    private static void ChangeRareSpyDataEnemies(List<int> rareSpyDataEnemyIds)
    {
        rareSpyDataEnemyIds.Clear();
        List<int> newRareSpyDateEnemyIds = _instance._enemiesRegistry
            .Where(l => l.IsRareSpyData)
            .Select(l => l.GameId)
            .ToList();
        rareSpyDataEnemyIds.AddRange(newRareSpyDateEnemyIds);
    }
}