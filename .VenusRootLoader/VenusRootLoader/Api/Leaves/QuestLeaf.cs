using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class QuestLeaf : Leaf, IEnemyPortraitSprite
{
    public sealed class QuestLanguageData
    {
        public string Name { get; set; } = "";
        public List<QuestDescriptionPage> PaginatedDescription { get; } = new();
        public string Sender { get; set; } = "";
    }

    public sealed class QuestDescriptionPage
    {
        public string Text { get; set; } = "<NO CONTENT>";
        public Branch<FlagLeaf>? RequiredFlag { get; set; }
    }

    internal QuestLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    int? IEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    IAssetLoader<Sprite> IEnemyPortraitSprite.PortraitSprite { get; set; }

    public IAssetLoader<Sprite> PortraitSprite
    {
        get => ((IEnemyPortraitSprite)this).PortraitSprite;
        set => ((IEnemyPortraitSprite)this).PortraitSprite = value;
    }

    public LocalizedData<QuestLanguageData> LocalizedData { get; } = new();
    public Branch<FlagLeaf>? TakenFlag { get; set; }
    public int Difficulty { get; set; }
    public List<Branch<FlagLeaf>> RequiredFlags { get; } = new();
    public List<Branch<AreaLeaf>> RequiredSeenAreas { get; } = new();
    public bool CanOnlyBeTakenAtUndergroundBar { get; set; }
}