using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

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
        public int? RequiredFlagGameId { get; set; }
    }

    int? IEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    WrappedSprite IEnemyPortraitSprite.WrappedSprite { get; set; } = new();

    public LocalizedData<QuestLanguageData> LocalizedData { get; } = new();
    public Branch<FlagLeaf>? TakenFlag { get; set; }
    public int Difficulty { get; set; }
    public List<Branch<FlagLeaf>> RequiredFlags { get; } = new();
    public List<Branch<AreaLeaf>> RequiredSeenAreas { get; } = new();
    public bool CanOnlyBeTakenAtUndergroundBar { get; set; }

    public Sprite PortraitSprite
    {
        get => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite!;
        set => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite = value;
    }
}