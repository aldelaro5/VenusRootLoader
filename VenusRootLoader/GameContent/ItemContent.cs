using VenusRootLoader.Patching.Resources.TextAsset.SerializableData;
using VenusRootLoader.Unity;

namespace VenusRootLoader.GameContent;

internal sealed class ItemContent : GameContent<int>
{
    internal WrappedSprite ItemSprite { get; } = new();
    internal ItemData ItemData { get; } = new();
    internal Dictionary<int, ItemLanguageData> ItemLanguageData { get; } = new();
}