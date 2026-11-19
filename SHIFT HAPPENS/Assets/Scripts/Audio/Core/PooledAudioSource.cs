using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class PooledAudioSource : MonoBehaviour
{
    private AudioSource source;
    private Action<PooledAudioSource> release;
    private Func<Vector3, Vector3> project;

    private float busVolume = 1f;
    private float volumeScale = 1f;
    private float fade = 1f;
    private float fadeSpeed;
    private bool fadingOut;

    private Transform follow;
    private bool following;

    public SoundData Sound { get; private set; }
    public AudioBus Bus { get; private set; }
    public bool InUse { get; private set; }
    public bool IsLooping => source != null && source.loop;
    public float StartedAt { get; private set; }
    public int Version { get; private set; }

    internal void Initialize(Action<PooledAudioSource> releaseCallback, Func<Vector3, Vector3> projectPosition)
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.dopplerLevel = 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        release = releaseCallback;
        project = projectPosition;
        gameObject.SetActive(false);
    }

    internal SoundHandle Play(SoundData sound, AudioClip clip, float pitch, float bus, bool positional, Vector3 position, Transform target)
    {
        Version++;
        InUse = true;
        Sound = sound;
        Bus = sound.Bus;
        busVolume = bus;
        volumeScale = 1f;
        fade = 1f;
        fadingOut = false;
        follow = target;
        following = target != null;

        gameObject.SetActive(true);
        transform.position = project(following ? target.position : position);

        source.clip = clip;
        source.loop = sound.Loop;
        source.pitch = pitch;
        source.spatialBlend = positional ? sound.SpatialBlend : 0f;
        source.minDistance = sound.MinDistance;
        source.maxDistance = sound.MaxDistance;
        ApplyVolume();

        StartedAt = Time.unscaledTime;
        source.Play();
        return new SoundHandle(this, Version);
    }

    internal bool Matches(int version) => InUse && Version == version;

    internal void SetBusVolume(float volume)
    {
        busVolume = volume;
        ApplyVolume();
    }

    internal void SetVolumeScale(float scale)
    {
        volumeScale = Mathf.Clamp01(scale);
        ApplyVolume();
    }

    internal void Stop(float fadeOut)
    {
        if (!InUse) return;
        if (fadeOut <= 0f)
        {
            Finish();
            return;
        }
        fadingOut = true;
        fadeSpeed = 1f / fadeOut;
    }

    private void LateUpdate()
    {
        if (!InUse) return;

        if (following)
        {
            if (follow == null)
            {
                following = false;
                if (source.loop)
                {
                    Finish();
                    return;
                }
            }
            else
            {
                transform.position = project(follow.position);
            }
        }

        if (fadingOut)
        {
            fade -= fadeSpeed * Time.unscaledDeltaTime;
            if (fade <= 0f)
            {
                Finish();
                return;
            }
            ApplyVolume();
        }

        if (!source.loop && !source.isPlaying)
            Finish();
    }

    private void ApplyVolume()
    {
        if (Sound == null) return;
        source.volume = Sound.Volume * volumeScale * fade * busVolume;
    }

    private void Finish()
    {
        source.Stop();
        source.clip = null;
        InUse = false;
        Sound = null;
        follow = null;
        following = false;
        fadingOut = false;
        gameObject.SetActive(false);
        release(this);
    }
}
