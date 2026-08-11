using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus(typeof(int))]
public sealed class DiscoveryLeaf : Leaf, IHasEnemyPortraitSprite
{
    public sealed class DiscoveryLanguageData
    {
        public string Name { get; set; } = "";
        public List<DiscoveryDescriptionPage> PaginatedDescription { get; init; } = new();
    }

    public sealed class DiscoveryDescriptionPage
    {
        public string Text { get; set; } = "<NO CONTENT>";
        public Branch<FlagLeaf>? RequiredFlag { get; set; }
    }

    internal DiscoveryLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }

    int? IHasEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    IAssetLoader<Sprite> IHasEnemyPortraitSprite.PortraitSprite { get; set; } = null!;

    public IAssetLoader<Sprite> PortraitSprite
    {
        get => ((IHasEnemyPortraitSprite)this).PortraitSprite;
        set => ((IHasEnemyPortraitSprite)this).PortraitSprite = value;
    }

    public LocalizedData<DiscoveryLanguageData> LocalizedData { get; } = new();

    [LeafInitializeFromNew]
    internal void InitializeFromNew(IAssetLoader<Sprite> portraitSprite)
    {
        PortraitSprite = portraitSprite;
    }
}