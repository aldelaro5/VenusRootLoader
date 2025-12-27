using UnityEngine;

namespace VenusRootLoader.Unity;

internal static class SharedAssets
{
    private static readonly Texture2D DummyItemOrMedalTexture;

    static SharedAssets()
    {
        Texture2D texture2D = new(64, 64, TextureFormat.RGBA32, false);
        texture2D.SetPixels(Enumerable.Repeat(Color.magenta, 64 * 64).ToArray());
        texture2D.Apply();
        DummyItemOrMedalTexture = texture2D;
    }

    internal static Sprite CreateDummyItemOrMedalSprite() => Sprite.Create(
        DummyItemOrMedalTexture,
        new Rect(0, 0, 64, 64),
        new Vector2(0.5f, 0.5f),
        60f);
}