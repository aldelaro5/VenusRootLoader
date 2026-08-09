using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.Unity.AssetLoading;

namespace VenusRootLoader.Unity;

internal static class SharedAssetLoaders
{
    private static readonly Texture2D DummyItemOrMedalTexture;

    static SharedAssetLoaders()
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

    internal static readonly IAssetLoader<AudioClip> DummyAudioClipLoader = new AssetLoaderFromDelegate<AudioClip>(() =>
        AudioClip.Create("", 1, 2, 48000, false));
}