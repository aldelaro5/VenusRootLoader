using UnityEngine;

namespace VenusRootLoader.Unity;

// TODO: Recheck if we need this still, it's possible we can just expose Sprite directly
internal sealed class WrappedSprite
{
    internal Sprite? Sprite { get; set; }
}