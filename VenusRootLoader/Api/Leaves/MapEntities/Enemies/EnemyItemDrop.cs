namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyItemDrop
{
    public required Branch<ItemLeaf> Item { get; set; }
    public required Branch<FlagLeaf>? RequiredFlag { get; set; }
}