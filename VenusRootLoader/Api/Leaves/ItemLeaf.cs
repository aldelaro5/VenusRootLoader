using VenusRootLoader.Api.TextAssetData.Items;
using VenusRootLoader.Api.Unity;

namespace VenusRootLoader.Api.Leaves;

public sealed class ItemLeaf : ILeaf<int>
{
    public required int GameId { get; init; }
    public required string NamedId { get; init; }
    public required string CreatorId { get; init; }

    public WrappedSprite ItemSprite { get; } = new();
    public ItemData ItemData { get; } = new();
    public Dictionary<int, ItemLanguageData> ItemLanguageData { get; } = new();
}