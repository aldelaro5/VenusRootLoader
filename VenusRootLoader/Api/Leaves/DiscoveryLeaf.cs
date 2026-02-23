using UnityEngine;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

public sealed class DiscoveryLeaf : ILeaf
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

    internal WrappedSprite WrappedSprite = new();

    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal int? EnemyPortraitsSpriteIndex { get; set; }

    public Dictionary<int, DiscoveryLanguageData> LanguageData { get; } = new();

    public Sprite PortraitSprite
    {
        get => WrappedSprite.Sprite;
        set => WrappedSprite.Sprite = value;
    }
}