// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

using UnityEngine;
using VenusRootLoader.Unity.CustomAudioClip;

namespace VenusRootLoader.Api;

public partial class Venus
{
    public AudioClip LoadAudioClipFromFile(string filePath) =>
        CustomAudioClipProvider.GetAudioClipFromFile(filePath, false, AudioFileFormat.AutoDetect);

    public AudioClip LoadAudioClipFromFile(string filePath, AudioFileFormat audioFileFormat) =>
        CustomAudioClipProvider.GetAudioClipFromFile(filePath, false, audioFileFormat);

    public AudioClip GetStreamedAudioClipFromFile(string filePath) =>
        CustomAudioClipProvider.GetAudioClipFromFile(filePath, true, AudioFileFormat.AutoDetect);

    public AudioClip GetStreamedAudioClipFromFile(string filePath, AudioFileFormat audioFileFormat) =>
        CustomAudioClipProvider.GetAudioClipFromFile(filePath, true, audioFileFormat);
}