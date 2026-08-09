using UnityEngine;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Unity.CustomAudioClip;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Api.Unity.AssetLoading;

public static class AssetLoader
{
    public static IAssetLoader<TObject> FromDelegate<TObject>(Func<TObject> loader) where TObject : Object
    {
        return new AssetLoaderFromDelegate<TObject>(loader);
    }

    public static IAssetLoader<TObject> FromResources<TObject>(string resourcesPath)
        where TObject : Object
    {
        return new AssetLoaderFromResources<TObject>(resourcesPath);
    }

    public static IAssetLoader<TObject> FromResourcesArray<TObject>(string resourcesPath, int arrayIndex)
        where TObject : Object
    {
        return new AssetLoaderFromResources<TObject>(resourcesPath, arrayIndex);
    }

    public static IAssetLoader<TObject> FromBundle<TObject>(AssetBundle bundle, string assetPathInBundle)
        where TObject : Object
    {
        return new AssetLoaderFromBundle<TObject>(bundle, assetPathInBundle);
    }

    public static IAssetLoader<AudioClip> AudioClipFromFile(string filePath)
    {
        return new AudioClipLoaderFromFile(filePath, false, AudioFileFormat.AutoDetect);
    }

    public static IAssetLoader<AudioClip> AudioClipFromFile(string filePath, AudioFileFormat audioFileFormat)
    {
        return new AudioClipLoaderFromFile(filePath, false, audioFileFormat);
    }

    public static IAssetLoader<AudioClip> AudioClipFromFileStreamed(string filePath)
    {
        return new AudioClipLoaderFromFile(filePath, true, AudioFileFormat.AutoDetect);
    }

    public static IAssetLoader<AudioClip> AudioClipFromFileStreamed(string filePath, AudioFileFormat audioFileFormat)
    {
        return new AudioClipLoaderFromFile(filePath, true, audioFileFormat);
    }
}