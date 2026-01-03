using UnityEngine;

namespace VenusRootLoader.Unity;

internal sealed class WrappedSprite
{
    internal Sprite Sprite { get; set; } = SharedAssets.CreateDummyItemOrMedalSprite();
}