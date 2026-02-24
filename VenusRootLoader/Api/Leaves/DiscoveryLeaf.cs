using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

public sealed class DiscoveryLeaf : ILeaf, IEnemyPortraitSprite
{
    public sealed class DiscoveryLanguageData
    {
        public string Name { get; set; } = "";
        public List<DiscoveryDescriptionPage> PaginatedDescription { get; init; } = new();
    }

    public sealed class DiscoveryDescriptionPage
    {
        public string Text { get; set; } = "<NO CONTENT>";
        public int? RequiredFlagGameId { get; set; }
    }

    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    int? IEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    WrappedSprite IEnemyPortraitSprite.WrappedSprite { get; set; } = new();

    public Dictionary<int, DiscoveryLanguageData> LanguageData { get; } = new();

    public Sprite PortraitSprite
    {
        get => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite;
        set => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite = value;
    }
}