using VenusRootLoader.Patching.Resources.TextAsset.SerializableData;
using VenusRootLoader.Unity;

namespace VenusRootLoader.GameContent;

internal sealed class ItemContent
{
    internal required int GameId { get; init; }
    internal WrappedSprite ItemSprite { get; } = new() { Sprite = SharedAssets.CreateDummyItemOrMedalSprite() };
    internal ItemData ItemData { get; } = new();
    internal Dictionary<int, ItemLanguageData> ItemLanguageData { get; } = new();
}