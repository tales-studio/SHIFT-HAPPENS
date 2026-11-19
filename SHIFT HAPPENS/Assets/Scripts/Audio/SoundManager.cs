using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour, ISfxService, IMusicService, IAudioVolumeService
{
    private static SoundManager instance;

    [Header("Sounds")]
    [SerializeField] private SoundLibrary library;

    [Header("Volume (can be changed live in Play Mode)")]
    [SerializeField] private AudioVolumeSettings volume = new AudioVolumeSettings();
    [SerializeField] private bool rememberPlayerSettings = true;

    [Header("Random pitch")]
    [SerializeField] private bool pitchVariationEnabled = true;
    [SerializeField, Range(0.5f, 1.5f)] private float outbreakPitchMultiplier = 0.85f;

    [Header("Music")]
    [SerializeField, Min(0f)] private float defaultMusicFade = 1.5f;

    [Header("Sound effect pool")]
    [SerializeField, Min(1)] private int initialPoolSize = 16;
    [SerializeField, Min(1)] private int maxPoolSize = 32;

    [Header("Top-down 2D")]
    [SerializeField] private AudioListener listener;
    [SerializeField] private bool matchListenerDepth = true;

    [Header("Lifetime")]
    [SerializeField] private bool persistAcrossScenes = true;

    private AudioSourcePool pool;
    private MusicPlayer music;
    private readonly Dictionary<SoundData, float> lastPlayTimes = new Dictionary<SoundData, float>();
    private readonly HashSet<UnityEngine.Object> warnedMissingClips = new HashSet<UnityEngine.Object>();
    private readonly HashSet<SoundId> warnedMissingIds = new HashSet<SoundId>();
    private bool initialized;

    public event Action VolumeChanged;

    public bool Distorted { get; set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (persistAcrossScenes)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        if (rememberPlayerSettings) volume.Load();

        pool = new AudioSourcePool(transform, initialPoolSize, maxPoolSize, ProjectToListener);
        music = new MusicPlayer(CreateMusicSource("Music A"), CreateMusicSource("Music B"));
        initialized = true;

        ApplyVolumes();
        AudioServices.Register(this, this, this);
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        AudioServices.Unregister(this);
        instance = null;
    }

    private void Update()
    {
        music.Tick(Time.unscaledDeltaTime);
    }

    private void OnValidate()
    {
        if (maxPoolSize < initialPoolSize) maxPoolSize = initialPoolSize;
        if (initialized && Application.isPlaying) ApplyVolumes();
    }

    [ContextMenu("Clear Saved Volume")]
    private void ClearSavedVolume()
    {
        AudioVolumeSettings.ClearSaved();
        Debug.Log("SoundManager: saved volume settings cleared. The inspector values will be used.");
    }

    public SoundHandle Play(SoundId id) => Play(Resolve(id));
    public SoundHandle PlayAt(SoundId id, Vector3 position) => PlayAt(Resolve(id), position);
    public SoundHandle PlayAttached(SoundId id, Transform target) => PlayAttached(Resolve(id), target);

    public SoundHandle Play(SoundData sound) => PlayInternal(sound, false, Vector3.zero, null);
    public SoundHandle PlayAt(SoundData sound, Vector3 position) => PlayInternal(sound, true, position, null);

    public SoundHandle PlayAttached(SoundData sound, Transform target)
    {
        if (target == null) return Play(sound);
        return PlayInternal(sound, true, target.position, target);
    }

    public void StopAll(float fadeOut = 0f)
    {
        if (initialized) pool.StopAll(fadeOut);
    }

    private SoundHandle PlayInternal(SoundData sound, bool positional, Vector3 position, Transform target)
    {
        if (!initialized || sound == null) return SoundHandle.None;

        if (!sound.Loop && !volume.IsEnabled(sound.Bus)) return SoundHandle.None;
        if (!PassesSpamRules(sound)) return SoundHandle.None;

        AudioClip clip = sound.PickClip(Distorted);
        if (clip == null)
        {
            if (warnedMissingClips.Add(sound))
                Debug.LogWarning($"SoundManager: '{sound.name}' has no audio clips assigned.", sound);
            return SoundHandle.None;
        }

        PooledAudioSource pooled = pool.Get();
        if (pooled == null) return SoundHandle.None;

        lastPlayTimes[sound] = Time.unscaledTime;
        return pooled.Play(sound, clip, ResolvePitch(sound), volume.GetBusVolume(sound.Bus), positional, position, target);
    }

    private bool PassesSpamRules(SoundData sound)
    {
        if (sound.Cooldown > 0f &&
            lastPlayTimes.TryGetValue(sound, out float lastTime) &&
            Time.unscaledTime - lastTime < sound.Cooldown)
            return false;

        if (sound.MaxSimultaneous > 0 && pool.CountPlaying(sound) >= sound.MaxSimultaneous)
            return false;

        return true;
    }

    private float ResolvePitch(SoundData sound)
    {
        float pitch = sound.PickPitch(pitchVariationEnabled);
        if (Distorted && sound.DistortDuringOutbreak && !sound.HasDistortedClips)
            pitch *= outbreakPitchMultiplier;
        return pitch;
    }

    public void PlayMusic(SoundId id, float fade = -1f)
    {
        if (id == SoundId.None)
        {
            StopMusic(fade);
            return;
        }
        PlayMusic(Resolve(id), fade);
    }

    public void PlayMusic(SoundData sound, float fade = -1f)
    {
        if (!initialized) return;
        if (sound == null)
        {
            StopMusic(fade);
            return;
        }

        AudioClip clip = sound.PickClip(Distorted);
        if (clip == null)
        {
            if (warnedMissingClips.Add(sound))
                Debug.LogWarning($"SoundManager: music '{sound.name}' has no audio clips assigned.", sound);
            return;
        }

        music.Play(sound, clip, ResolvePitch(sound), fade < 0f ? defaultMusicFade : fade);
    }

    public void StopMusic(float fade = -1f)
    {
        if (initialized) music.Stop(fade < 0f ? defaultMusicFade : fade);
    }

    public float MasterVolume
    {
        get => volume.MasterVolume;
        set { volume.MasterVolume = value; OnPlayerChangedVolume(); }
    }

    public bool MusicEnabled
    {
        get => volume.MusicEnabled;
        set { volume.MusicEnabled = value; OnPlayerChangedVolume(); }
    }

    public float MusicVolume
    {
        get => volume.MusicVolume;
        set { volume.MusicVolume = value; OnPlayerChangedVolume(); }
    }

    public bool SfxEnabled
    {
        get => volume.SfxEnabled;
        set { volume.SfxEnabled = value; OnPlayerChangedVolume(); }
    }

    public float SfxVolume
    {
        get => volume.SfxVolume;
        set { volume.SfxVolume = value; OnPlayerChangedVolume(); }
    }

    public bool UiEnabled
    {
        get => volume.UiEnabled;
        set { volume.UiEnabled = value; OnPlayerChangedVolume(); }
    }

    public float UiVolume
    {
        get => volume.UiVolume;
        set { volume.UiVolume = value; OnPlayerChangedVolume(); }
    }

    private void OnPlayerChangedVolume()
    {
        ApplyVolumes();
        if (rememberPlayerSettings) volume.Save();
    }

    private void ApplyVolumes()
    {
        if (!initialized) return;

        music.SetBusVolume(volume.GetBusVolume(AudioBus.Music));
        foreach (PooledAudioSource pooled in pool.Active)
            pooled.SetBusVolume(volume.GetBusVolume(pooled.Bus));

        VolumeChanged?.Invoke();
    }

    private SoundData Resolve(SoundId id)
    {
        if (id == SoundId.None) return null;

        if (library == null)
        {
            if (warnedMissingIds.Add(SoundId.None))
                Debug.LogWarning("SoundManager: no SoundLibrary assigned.", this);
            return null;
        }

        if (library.TryGet(id, out SoundData sound)) return sound;

        if (warnedMissingIds.Add(id))
            Debug.LogWarning($"SoundManager: {id} is not in the SoundLibrary.", library);
        return null;
    }

    private Vector3 ProjectToListener(Vector3 position)
    {
        if (!matchListenerDepth) return position;
        if (listener == null) listener = FindFirstObjectByType<AudioListener>();
        if (listener != null) position.z = listener.transform.position.z;
        return position;
    }

    private AudioSource CreateMusicSource(string sourceName)
    {
        GameObject go = new GameObject(sourceName);
        go.transform.SetParent(transform, false);
        return go.AddComponent<AudioSource>();
    }
}
