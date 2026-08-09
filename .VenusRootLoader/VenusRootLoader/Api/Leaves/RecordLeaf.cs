using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus(typeof(int))]
public sealed class RecordLeaf : Leaf, IEnemyPortraitSprite
{
    public sealed class RecordLanguageData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    internal RecordLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    int? IEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    IAssetLoader<Sprite> IEnemyPortraitSprite.PortraitSprite { get; set; }

    public IAssetLoader<Sprite> PortraitSprite
    {
        get => ((IEnemyPortraitSprite)this).PortraitSprite;
        set => ((IEnemyPortraitSprite)this).PortraitSprite = value;
    }

    public LocalizedData<RecordLanguageData> LocalizedData { get; } = new();
}