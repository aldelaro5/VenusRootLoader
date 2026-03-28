using UnityEngine;

namespace VenusRootLoader.Unity.CustomAudioClip;

/// <summary>
/// An audio file format used for loading <see cref="AudioClip"/>.
/// </summary>
public enum AudioFileFormat
{
    /// <summary>
    /// Automatically determines the format based on the file's extension.
    /// </summary>
    AutoDetect,

    /// <summary>
    /// Microsoft WAV.
    /// </summary>
    Wav,

    /// <summary>
    /// Ogg vorbis.
    /// </summary>
    Ogg,

    /// <summary>
    /// MP3 MPEG.
    /// </summary>
    Mp3,

    /// <summary>
    /// Free Lossless Audio Coded FLAC.
    /// </summary>
    Flac,

    /// <summary>
    /// Audio Interchange File Format AIFF
    /// </summary>
    Aiff,

    /// <summary>
    /// ScreamTracker 3.
    /// </summary>
    /// <remarks>This format cannot be streamed and must be loaded fully</remarks>
    S3M,

    /// <summary>
    /// Impulse tracker.
    /// </summary>
    /// <remarks>This format cannot be streamed and must be loaded fully</remarks>
    It,

    /// <summary>
    /// Protracker / Fasttracker MOD.
    /// </summary>
    /// <remarks>This format cannot be streamed and must be loaded fully</remarks>
    Mod,

    /// <summary>
    /// FastTracker 2 XM.
    /// </summary>
    /// <remarks>This format cannot be streamed and must be loaded fully</remarks>
    Xm
}