using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class ItemUsageLeaf : Leaf
{
    public delegate void ItemUsageHandler(MainManager.DoItemEffectContext context);

    public delegate void BattleItemUsageHandler(BattleControl battle, BattleControl.UseItemBattleEffectContext context);

    internal ItemUsageLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public ItemUsageHandler? UsageHandler { get; set; }
    public BattleItemUsageHandler? BattleUsageHandler { get; set; }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(ItemUsageHandler? usageHandler, BattleItemUsageHandler? battleUsageHandler)
    {
        UsageHandler = usageHandler;
        BattleUsageHandler = battleUsageHandler;
    }
}