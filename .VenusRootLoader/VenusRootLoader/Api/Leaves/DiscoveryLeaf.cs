using UnityEngine;
using VenusRootLoader.Api.Unity.AssetLoading;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus(typeof(int))]
public sealed class DiscoveryLeaf : Leaf, IEnemyPortraitSprite
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

    int? IEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    IAssetLoader<Sprite> IEnemyPortraitSprite.PortraitSprite { get; set; }

    public IAssetLoader<Sprite> PortraitSprite
    {
        get => ((IEnemyPortraitSprite)this).PortraitSprite;
        set => ((IEnemyPortraitSprite)this).PortraitSprite = value;
    }

    public LocalizedData<DiscoveryLanguageData> LocalizedData { get; } = new();
}