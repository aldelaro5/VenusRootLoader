using NAudio.Wave;

namespace VenusRootLoader.Unity.CustomAudioClip;

internal sealed class StreamedNAudio
{
    private readonly ISampleProvider _sampleProvider;
    private readonly WaveStream _waveStream;

    internal StreamedNAudio(ISampleProvider sampleProvider, WaveStream waveStream)
    {
        _sampleProvider = sampleProvider;
        _waveStream = waveStream;
    }

    internal void OnRead(float[] data) => _sampleProvider.Read(data, 0, data.Length);
    internal void OnSeek(int position) => _waveStream.Position = position;
}