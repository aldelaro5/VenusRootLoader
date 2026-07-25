using CommunityToolkit.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

namespace VenusRootLoader.Unity.CustomAudioClip;

internal static class UnityAudioClipLoader
{
    public static AudioClip LoadFromFile(string filePath, AudioFileFormat format, bool isStreamed)
    {
        AudioType audioType = GetAudioTypeFromAudioFileFormat(format);
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType);
        DownloadHandlerAudioClip downloadHandler = (DownloadHandlerAudioClip)webRequest.downloadHandler;
        if (isStreamed)
            downloadHandler.streamAudio = true;

        webRequest.SendWebRequest();
        while (isStreamed ? webRequest.downloadedBytes == 0 : !webRequest.isDone)
            continue;

        AudioClip audioClip = isStreamed
            ? downloadHandler.audioClip
            : DownloadHandlerAudioClip.GetContent(webRequest);
        return audioClip;
    }

    private static AudioType GetAudioTypeFromAudioFileFormat(AudioFileFormat audioFileFormat)
    {
        return audioFileFormat switch
        {
            AudioFileFormat.Wav => AudioType.WAV,
            AudioFileFormat.Ogg => AudioType.OGGVORBIS,
            AudioFileFormat.Aiff => AudioType.AIFF,
            AudioFileFormat.S3M => AudioType.S3M,
            AudioFileFormat.It => AudioType.IT,
            AudioFileFormat.Mod => AudioType.MOD,
            AudioFileFormat.Xm => AudioType.XM,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<AudioType>(
                nameof(audioFileFormat),
                $"Invalid or unsupported audio file format by Unity: {audioFileFormat}")
        };
    }
}