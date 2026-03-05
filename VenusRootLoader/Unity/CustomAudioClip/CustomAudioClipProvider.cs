using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using UnityEngine;

namespace VenusRootLoader.Unity.CustomAudioClip;

internal interface ICustomAudioClipProvider
{
    AudioClip GetAudioClipFromFile(string filePath, bool isStreamed, AudioFileFormat audioFileFormat);
}

internal sealed class CustomAudioClipProvider : ICustomAudioClipProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<CustomAudioClipProvider> _logger;

    public CustomAudioClipProvider(IFileSystem fileSystem, ILogger<CustomAudioClipProvider> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public AudioClip GetAudioClipFromFile(string filePath, bool isStreamed, AudioFileFormat audioFileFormat)
    {
        if (!_fileSystem.File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        if (audioFileFormat == AudioFileFormat.AutoDetect)
            audioFileFormat = DetermineFormatFromFileExtension(filePath);

        switch (audioFileFormat)
        {
            case AudioFileFormat.Mp3:
            case AudioFileFormat.Flac:
                return NAudioAudioClipLoader.LoadFromFile(filePath, audioFileFormat, isStreamed);
            case AudioFileFormat.Wav:
            case AudioFileFormat.Ogg:
            case AudioFileFormat.Aiff:
                return UnityAudioClipLoader.LoadFromFile(filePath, audioFileFormat, isStreamed);
            case AudioFileFormat.S3M:
            case AudioFileFormat.It:
            case AudioFileFormat.Mod:
            case AudioFileFormat.Xm:
                if (isStreamed)
                {
                    _logger.LogWarning(
                        "The tracker file {filePath} cannot be streamed so it will be loaded fully",
                        filePath);
                }

                return UnityAudioClipLoader.LoadFromFile(filePath, audioFileFormat, false);
            default:
                return ThrowHelper.ThrowArgumentOutOfRangeException<AudioClip>(
                    nameof(audioFileFormat),
                    $"Invalid {nameof(AudioFileFormat)}: {audioFileFormat}");
        }
    }

    private AudioFileFormat DetermineFormatFromFileExtension(string filePath) =>
        _fileSystem.Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".wav" => AudioFileFormat.Wav,
            ".ogg" => AudioFileFormat.Ogg,
            ".mp3" => AudioFileFormat.Mp3,
            ".flac" => AudioFileFormat.Flac,
            ".aiff" => AudioFileFormat.Aiff,
            ".s3m" => AudioFileFormat.S3M,
            ".it" => AudioFileFormat.It,
            ".mod" => AudioFileFormat.Mod,
            ".xm" => AudioFileFormat.Xm,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<AudioFileFormat>(
                nameof(filePath),
                $"Couldn't determine the {nameof(AudioFileFormat)} from the file path {filePath}")
        };
}