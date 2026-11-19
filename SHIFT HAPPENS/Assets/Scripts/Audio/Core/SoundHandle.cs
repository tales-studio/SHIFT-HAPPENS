public readonly struct SoundHandle
{
    private readonly PooledAudioSource source;
    private readonly int version;

    internal SoundHandle(PooledAudioSource source, int version)
    {
        this.source = source;
        this.version = version;
    }

    public static SoundHandle None => default;

    public bool IsPlaying => source != null && source.Matches(version);
    public bool IsLooping => IsPlaying && source.IsLooping;

    public void Stop(float fadeOut = 0f)
    {
        if (IsPlaying) source.Stop(fadeOut);
    }

    public void SetVolume(float volumeScale)
    {
        if (IsPlaying) source.SetVolumeScale(volumeScale);
    }
}
