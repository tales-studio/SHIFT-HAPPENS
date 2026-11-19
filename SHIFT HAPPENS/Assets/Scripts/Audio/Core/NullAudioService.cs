using System;
using UnityEngine;

public sealed class NullAudioService : ISfxService, IMusicService, IAudioVolumeService
{
    public static readonly NullAudioService Instance = new NullAudioService();

    private NullAudioService() { }

    public bool Distorted { get; set; }

    public SoundHandle Play(SoundId id) => SoundHandle.None;
    public SoundHandle Play(SoundData sound) => SoundHandle.None;
    public SoundHandle PlayAt(SoundId id, Vector3 position) => SoundHandle.None;
    public SoundHandle PlayAt(SoundData sound, Vector3 position) => SoundHandle.None;
    public SoundHandle PlayAttached(SoundId id, Transform target) => SoundHandle.None;
    public SoundHandle PlayAttached(SoundData sound, Transform target) => SoundHandle.None;
    public void StopAll(float fadeOut = 0f) { }

    public void PlayMusic(SoundId id, float fade = -1f) { }
    public void PlayMusic(SoundData sound, float fade = -1f) { }
    public void StopMusic(float fade = -1f) { }

    public float MasterVolume { get => 1f; set { } }
    public bool MusicEnabled { get => true; set { } }
    public float MusicVolume { get => 1f; set { } }
    public bool SfxEnabled { get => true; set { } }
    public float SfxVolume { get => 1f; set { } }
    public bool UiEnabled { get => true; set { } }
    public float UiVolume { get => 1f; set { } }

    public event Action VolumeChanged { add { } remove { } }
}
