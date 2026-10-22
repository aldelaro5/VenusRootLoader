using System.Collections;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus(leafKeyType: typeof(AnimIdLeaf))]
public sealed class BattleHelperLeaf : Leaf
{
    public delegate bool JoinBattlePredicate();

    public delegate bool PresenceInHelperMedalPoolPredicate();

    public delegate IEnumerator AttackHandler(BattleControl battle, BattleControl.AIAttackContext context);

    internal BattleHelperLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public Branch<AnimIdLeaf> AnimId { get; internal set; } = null!;
    public int StartingAnimstate { get; set; }

    public JoinBattlePredicate? PredicateJoinBattle { get; set; }
    public PresenceInHelperMedalPoolPredicate PredicateIsInHelperMedalPool { get; set; } = null!;
    public AttackHandler Attack { get; set; } = null!;

    [LeafInitializeFromNew(true)]
    internal void InitializeFromNew(
        Branch<AnimIdLeaf> animId,
        JoinBattlePredicate? predicateJoinBattle,
        PresenceInHelperMedalPoolPredicate predicateIsInHelperMedalPool,
        AttackHandler attackHandler)
    {
        AnimId = animId;
        PredicateJoinBattle = predicateJoinBattle;
        PredicateIsInHelperMedalPool = predicateIsInHelperMedalPool;
        Attack = attackHandler;
    }
}