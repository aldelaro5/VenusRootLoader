namespace VenusRootLoader.Api.Leaves;

// TODO: Support attack numbers higher than 9 for Attack cards due to number sprites rendering issue in CreateCard
public sealed class SpyCardLeaf : Leaf
{
    public sealed class SpyCardEffect
    {
        public CardGame.Effects Effect { get; set; }
        public int FirstValue { get; set; }
        public int SecondValue { get; set; }
    }

    public sealed class SpyCardLanguageData
    {
        public string Description { get; set; } = "";
        public float HorizontalNameSize { get; set; } = 1;
    }

    internal SpyCardLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public LocalizedData<SpyCardLanguageData> LocalizedData { get; } = new();

    public int TpCost { get; set; }
    public int Attack { get; set; }
    public Branch<EnemyLeaf> Enemy { get; set; }
    internal float UnusedHorizontalNameSize { get; set; } = 1.0f;
    public CardGame.Type Type { get; set; }
    public List<SpyCardEffect> Effects { get; } = new();
    public List<CardGame.Tribe> Tribes { get; } = new();
}