using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;
using VenusRootLoader.Unity;

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
    WrappedSprite IEnemyPortraitSprite.WrappedSprite { get; set; } = new();

    public LocalizedData<DiscoveryLanguageData> LocalizedData { get; } = new();

    public Sprite PortraitSprite
    {
        get => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite!;
        set => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite = value;
    }
}