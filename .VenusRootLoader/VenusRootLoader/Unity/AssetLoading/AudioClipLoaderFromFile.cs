using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.Unity.CustomAudioClip;

namespace VenusRootLoader.Unity.AssetLoading;

internal sealed class AudioClipLoaderFromFile : IAssetLoader<AudioClip>
{
    private readonly string _filePath;
    private readonly bool _isStreamed;
    private readonly AudioFileFormat _audioFileFormat;

    internal AudioClipLoaderFromFile(string filePath, bool isStreamed, AudioFileFormat audioFileFormat)
    {
        _filePath = filePath;
        _isStreamed = isStreamed;
        _audioFileFormat = audioFileFormat;
    }

    public AudioClip LoadAsset() =>
        CustomAudioClipProvider.GetAudioClipFromFile(_filePath, _isStreamed, _audioFileFormat);
}