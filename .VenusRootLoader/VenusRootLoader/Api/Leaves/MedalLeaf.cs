using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus(typeof(MainManager.BadgeTypes))]
public sealed class MedalLeaf : Leaf
{
    public sealed class MedalEffect
    {
        public MainManager.BadgeEffects Effect { get; set; }
        public int Value { get; set; }
    }

    public sealed class MedalLanguageData
    {
        public string Name { get; set; } = "<NO NAME>";
        public string Description { get; set; } = "<NO DESCRIPTION>";
        public string Prepender { get; set; } = "<NO PREPENDER>";
    }

    internal MedalLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    internal int Items1SpriteIndex { get; set; } = -1;

    public int MpCost { get; set; }
    public bool IsPartyEquip { get; set; }
    public List<MedalEffect> Effects { get; } = new();
    public int BuyingPriceRegularBerries { get; set; }
    public int BuyingPriceCrystalBerries { get; set; }
    public LocalizedData<MedalLanguageData> LocalizedData { get; } = new();

    public IAssetLoader<Sprite> Sprite { get; set; } = null!;

    [LeafInitializeFromNew]
    internal void InitializeFromNew(IAssetLoader<Sprite> sprite)
    {
        Sprite = sprite;
    }
}