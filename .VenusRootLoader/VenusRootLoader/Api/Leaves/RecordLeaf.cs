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

    public delegate bool AutoUnlockPredicate(MainManager mainManager);

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
    public AutoUnlockPredicate? AutoUnlockConditions { get; set; }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(
        IAssetLoader<Sprite> portraitSprite,
        AutoUnlockPredicate? autoUnlockConditions)
    {
        PortraitSprite = portraitSprite;
        AutoUnlockConditions = autoUnlockConditions;
    }
}