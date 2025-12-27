using VenusRootLoader.Patching.Resources.TextAsset.SerializableData;
using VenusRootLoader.Unity;

namespace VenusRootLoader.GameContent;

internal sealed class ItemContent : IGameContent<int>
{
    public required int GameId { get; internal init; }
    internal WrappedSprite ItemSprite { get; } = new();
    internal ItemData ItemData { get; } = new();
    internal Dictionary<int, ItemLanguageData> ItemLanguageData { get; } = new();
}