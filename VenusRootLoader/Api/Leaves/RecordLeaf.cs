using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

public sealed class RecordLeaf : ILeaf, IEnemyPortraitSprite
{
    public sealed class RecordLanguageData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    int? IEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    WrappedSprite IEnemyPortraitSprite.WrappedSprite { get; set; } = new();

    public Dictionary<int, RecordLanguageData> LanguageData { get; } = new();

    public Sprite PortraitSprite
    {
        get => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite;
        set => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite = value;
    }
}