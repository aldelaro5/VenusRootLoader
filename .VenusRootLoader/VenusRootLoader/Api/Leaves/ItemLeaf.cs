using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class ItemLeaf : Leaf
{
    public sealed class ItemUse
    {
        public MainManager.ItemUsage Effect { get; set; }
        public int Value { get; set; }
    }

    public sealed class ItemLanguageData
    {
        public string Name { get; set; } = "<NO NAME>";
        public string UnusedDescription { get; set; } = "";
        public string Description { get; set; } = "<NO DESCRIPTION>";
        public string? Prepender { get; set; }
    }

    internal ItemLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public List<ItemUse> Effects { get; } = new();
    public LocalizedData<ItemLanguageData> LocalizedData { get; } = new();

    public IAssetLoader<Sprite> Sprite { get; set; } = null!;

    public int BuyingPrice { get; set; }
    public BattleControl.AttackArea Target { get; set; }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(IAssetLoader<Sprite> sprite)
    {
        Sprite = sprite;
    }
}