using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus(typeof(int))]
public sealed class RecordLeaf : Leaf, IHasEnemyPortraitSprite
{
    public sealed class RecordLanguageData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    internal RecordLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    int? IHasEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    IAssetLoader<Sprite> IHasEnemyPortraitSprite.PortraitSprite { get; set; } = null!;

    public IAssetLoader<Sprite> PortraitSprite
    {
        get => ((IHasEnemyPortraitSprite)this).PortraitSprite;
        set => ((IHasEnemyPortraitSprite)this).PortraitSprite = value;
    }

    public LocalizedData<RecordLanguageData> LocalizedData { get; } = new();

    [LeafInitializeFromNew]
    internal void InitializeFromNew(IAssetLoader<Sprite> portraitSprite)
    {
        PortraitSprite = portraitSprite;
    }
}