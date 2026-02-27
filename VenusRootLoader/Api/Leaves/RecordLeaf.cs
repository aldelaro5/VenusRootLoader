using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

public sealed class RecordLeaf : Leaf, IEnemyPortraitSprite
{
    public sealed class RecordLanguageData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    int? IEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    WrappedSprite IEnemyPortraitSprite.WrappedSprite { get; set; } = new();

    public Dictionary<int, RecordLanguageData> LanguageData { get; } = new();

    public Sprite PortraitSprite
    {
        get => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite;
        set => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite = value;
    }
}