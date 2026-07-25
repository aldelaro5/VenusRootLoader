using CommunityToolkit.Diagnostics;
using NAudio.Flac;
using NAudio.Wave;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace VenusRootLoader.Unity.CustomAudioClip;

[SuppressMessage("System.IO.Abstractions", "IO0006:Replace Path class with IFileSystem.Path for improved testability")]
internal static class NAudioAudioClipLoader
{
    public static AudioClip LoadFromFile(string filePath, AudioFileFormat format, bool isStreamed)
    {
        WaveStream waveStream = GetWaveStreamFromFile(filePath, format);
        ISampleProvider sampleProvider = waveStream.ToSampleProvider();
        int sampleFrames = (int)(waveStream.Length / waveStream.BlockAlign);

        if (isStreamed)
        {
            StreamedNAudio streamedNAudio = new(sampleProvider, waveStream);
            return AudioClip.Create(
                Path.GetFileNameWithoutExtension(filePath),
                sampleFrames,
                sampleProvider.WaveFormat.Channels,
                sampleProvider.WaveFormat.SampleRate,
                true,
                streamedNAudio.OnRead,
                streamedNAudio.OnSeek);
        }

        int lengthSamples = sampleFrames * waveStream.WaveFormat.Channels;
        float[] samples = ReadAllSamplesFromSampleProvider(sampleProvider, lengthSamples);

        AudioClip clip = AudioClip.Create(
            Path.GetFileNameWithoutExtension(filePath),
            sampleFrames,
            sampleProvider.WaveFormat.Channels,
            sampleProvider.WaveFormat.SampleRate,
            false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static float[] ReadAllSamplesFromSampleProvider(ISampleProvider sampleProvider, int lengthSamples)
    {
        float[] samples = new float[lengthSamples];
        int offset = 0;
        int totalRead = 0;
        int read = 0;
        while (totalRead <= lengthSamples)
        {
            read += sampleProvider.Read(samples, offset, lengthSamples);
            offset += read;
            totalRead += read;
        }

        return samples;
    }

    private static WaveStream GetWaveStreamFromFile(string filePath, AudioFileFormat audioFileFormat)
    {
        return audioFileFormat switch
        {
            AudioFileFormat.Mp3 => new Mp3FileReaderBase(filePath, f => new AcmMp3FrameDecompressor(f)),
            AudioFileFormat.Flac => new FlacReader(filePath),
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<WaveStream>(
                nameof(audioFileFormat),
                $"Invalid or unsupported audio file format by NAudio: {audioFileFormat}")
        };
    }
}