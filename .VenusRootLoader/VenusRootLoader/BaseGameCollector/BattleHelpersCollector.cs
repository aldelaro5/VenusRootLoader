using Microsoft.Extensions.Logging;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using VenusRootLoader.Api.Extensions;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BattleHelpersCollector : IBaseGameCollector
{
    private readonly ILogger<BattleHelpersCollector> _logger;
    private readonly ILeavesRegistry<BattleHelperLeaf> _battleHelpersRegistry;
    private readonly ILeavesRegistry<AnimIdLeaf> _animIdsRegistry;

    public BattleHelpersCollector(
        ILogger<BattleHelpersCollector> logger,
        ILeavesRegistry<BattleHelperLeaf> battleHelpersRegistry,
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry)
    {
        _logger = logger;
        _battleHelpersRegistry = battleHelpersRegistry;
        _animIdsRegistry = animIdsRegistry;
    }

    public void CollectBaseGameData()
    {
        Dictionary<string, MethodInfo> helperPredicates = RootCollector.GetLeavesMethods(
            typeof(BattleControl),
            nameof(BattleControl.StartBattle),
            "HelperPredicate");
        Dictionary<string, MethodInfo> helperMedalPoolPredicates = RootCollector.GetLeavesMethods(
            typeof(BattleControl),
            nameof(BattleControl.StartBattle),
            "HelperMedalPoolPredicate");
        Dictionary<string, MethodInfo> aiAttackHandlers = RootCollector.GetLeavesMethods(
            typeof(BattleControl),
            nameof(BattleControl.AIAttack));

        MethodInfo startBattle = typeof(BattleControl).MethodByName(nameof(BattleControl.StartBattle))!;
        MethodInfo startBattleMoveNextMethod = startBattle.GetMoveNextForCoroutineMethod();
        using DynamicMethodDefinition dmd = new(startBattleMoveNextMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        cursor.GotoNext(x => x.MatchStfld<BattleControl>(nameof(BattleControl.aiparty)));
        cursor.GotoNext(x => x.MatchLdfld<BattleControl>(nameof(BattleControl.aiparty)));

        Dictionary<int, int> helperStartingAnimStatesByAnimId = [];
        while (cursor.TryGotoNext(x => x.MatchCallOrCallvirt<List<int[]>>(nameof(List<>.Add))))
        {
            int indexAdd = cursor.Index;
            cursor.GotoPrev(x => x.MatchNewarr(out _));
            cursor.GotoNext(x => x.MatchStelemI4());
            int animId = cursor.Instrs[cursor.Index - 1].GetInt();
            cursor.Index++;
            cursor.GotoNext(x => x.MatchStelemI4());
            int startingAnimState = cursor.Index < indexAdd ? cursor.Instrs[cursor.Index - 1].GetInt() : 0;
            helperStartingAnimStatesByAnimId.Add(animId, startingAnimState);

            cursor.Index = indexAdd + 1;
        }

        foreach (KeyValuePair<int, int> helper in helperStartingAnimStatesByAnimId)
        {
            AnimIdLeaf animIdLeaf = _animIdsRegistry.GetByGameId(helper.Key);
            BattleHelperLeaf battleHelperLeaf = _battleHelpersRegistry.RegisterExisting(
                animIdLeaf.GameId,
                animIdLeaf.NamedId);
            battleHelperLeaf.AnimId = animIdLeaf;
            battleHelperLeaf.StartingAnimstate = helper.Value;

            if (helperPredicates.TryGetValue(battleHelperLeaf.NamedId, out MethodInfo methodHelper))
            {
                battleHelperLeaf.PredicateJoinBattle =
                    RootCollector.CreateOpenDelegateFromMethod<BattleHelperLeaf.JoinBattlePredicate>(methodHelper);
            }

            if (helperMedalPoolPredicates.TryGetValue(battleHelperLeaf.NamedId, out MethodInfo methodMedalPool))
            {
                battleHelperLeaf.PredicateIsInHelperMedalPool =
                    RootCollector.CreateOpenDelegateFromMethod<BattleHelperLeaf.PresenceInHelperMedalPoolPredicate>(
                        methodMedalPool);
            }

            if (aiAttackHandlers.TryGetValue(battleHelperLeaf.NamedId, out MethodInfo methodAttack))
            {
                battleHelperLeaf.Attack =
                    RootCollector.CreateOpenDelegateFromMethod<BattleHelperLeaf.AttackHandler>(methodAttack);
            }
        }

        RootCollector.LogCollectedAmount(_logger, _battleHelpersRegistry, _battleHelpersRegistry.Count);
    }
}