using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace VenusRootLoader.Unity.CustomAudioClip;

/// <summary>
/// A service provided to buds that allows them to obtain an <see cref="AudioClip"/> from an external audio file.
/// It supports a wide variety of common audio file formats and the ability to stream the file or load it fully.
/// The implementation uses a mix of <see cref="UnityEngine.Networking.UnityWebRequestMultimedia"/> and
/// <see cref="NAudio.Wave.WaveStream"/> depending on the file format (the former is prefered since it's faster, but it
/// doesn't support Mp3 and Flac while the latter supports them).
/// </summary>
[SuppressMessage("System.IO.Abstractions", "IO0002:Replace File class with IFileSystem.File for improved testability")]
[SuppressMessage("System.IO.Abstractions", "IO0006:Replace Path class with IFileSystem.Path for improved testability")]
internal static class CustomAudioClipProvider
{
    /// <summary>
    /// Obtains an <see cref="AudioClip"/> from an external audio file.
    /// </summary>
    /// <param name="filePath">The full path of the audio file.</param>
    /// <param name="isStreamed">Tells if the <see cref="AudioClip"/> should be streamed or loaded fully.</param>
    /// <param name="audioFileFormat">The format to use for loading the audio file.</param>
    /// <returns>The newly created <see cref="AudioClip"/></returns>
    /// <remarks>If <paramref name="audioFileFormat"/> is a tracker format, it will be loaded fully regardless of the
    /// value of <paramref name="isStreamed"/></remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="audioFileFormat"/> is
    /// <see cref="AudioFileFormat.AutoDetect"/> and the file format couldn't be determined from the
    /// <paramref name="filePath"/>'s extension.</exception>
    internal static AudioClip GetAudioClipFromFile(string filePath, bool isStreamed, AudioFileFormat audioFileFormat)
    {
        if (!File.Exists(filePath))
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
                return UnityAudioClipLoader.LoadFromFile(filePath, audioFileFormat, false);
            default:
                return ThrowHelper.ThrowArgumentOutOfRangeException<AudioClip>(
                    nameof(audioFileFormat),
                    $"Invalid {nameof(AudioFileFormat)}: {audioFileFormat}");
        }
    }

    private static AudioFileFormat DetermineFormatFromFileExtension(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
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