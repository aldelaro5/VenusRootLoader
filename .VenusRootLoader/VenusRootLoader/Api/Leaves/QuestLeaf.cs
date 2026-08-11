using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class QuestLeaf : Leaf, IHasEnemyPortraitSprite
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

    int? IHasEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    IAssetLoader<Sprite> IHasEnemyPortraitSprite.PortraitSprite { get; set; } = null!;

    public IAssetLoader<Sprite> PortraitSprite
    {
        get => ((IHasEnemyPortraitSprite)this).PortraitSprite;
        set => ((IHasEnemyPortraitSprite)this).PortraitSprite = value;
    }

    public LocalizedData<QuestLanguageData> LocalizedData { get; } = new();
    public Branch<FlagLeaf>? TakenFlag { get; set; }
    public int Difficulty { get; set; }
    public List<Branch<FlagLeaf>> RequiredFlags { get; } = new();
    public List<Branch<AreaLeaf>> RequiredSeenAreas { get; } = new();
    public bool CanOnlyBeTakenAtUndergroundBar { get; set; }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(IAssetLoader<Sprite> portraitSprite)
    {
        PortraitSprite = portraitSprite;
    }
}