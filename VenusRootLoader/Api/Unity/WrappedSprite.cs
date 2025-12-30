using UnityEngine;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Unity;

public sealed class WrappedSprite
{
    public Sprite Sprite { get; set; } = SharedAssets.CreateDummyItemOrMedalSprite();
}