namespace VenusRootLoader.Api.Leaves;

internal sealed class SpyCardLeaf : Leaf
{
    internal sealed class SpyCardEffect
    {
        internal CardGame.Effects Effect { get; set; }
        internal int FirstValue { get; set; }
        internal int SecondValue { get; set; }
    }

    internal Dictionary<int, string> Description { get; } = new();
    internal Dictionary<int, float> HorizontalNameSize { get; } = new();

    internal int TpCost { get; set; }
    internal int Attack { get; set; }
    internal int EnemyGameId { get; set; }
    internal float UnusedHorizontalNameSize { get; set; } = 1.0f;
    internal CardGame.Type Type { get; set; }
    internal List<SpyCardEffect> Effects { get; } = new();
    internal List<CardGame.Tribe> Tribes { get; } = new();
}