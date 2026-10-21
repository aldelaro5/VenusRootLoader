using System.Collections;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

// TODO: This leaf shouldn't be part of AnimIdLeaf, but it's also unique BY anim id. See if we can have a concept of a "sub" leaf.
[ExposeFromVenus]
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

    [LeafInitializeFromNew]
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