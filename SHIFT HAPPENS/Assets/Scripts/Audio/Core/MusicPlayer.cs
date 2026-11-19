using UnityEngine;

public sealed class MusicPlayer
{
    private AudioSource current;
    private AudioSource previous;
    private SoundData currentSound;
    private SoundData previousSound;
    private float currentFade;
    private float previousFade;
    private float fadeSpeed = float.PositiveInfinity;
    private float busVolume = 1f;

    public SoundData CurrentSound => currentSound;

    public MusicPlayer(AudioSource sourceA, AudioSource sourceB)
    {
        current = sourceA;
        previous = sourceB;
        Configure(sourceA);
        Configure(sourceB);
    }

    public void Play(SoundData sound, AudioClip clip, float pitch, float fadeDuration)
    {
        if (sound == currentSound && current.isPlaying) return;

        SwapSources();
        previousSound = currentSound;
        previousFade = currentFade;

        currentSound = sound;
        currentFade = 0f;
        fadeSpeed = fadeDuration > 0f ? 1f / fadeDuration : float.PositiveInfinity;

        current.clip = clip;
        current.loop = sound.Loop;
        current.pitch = pitch;
        current.Play();
        ApplyVolumes();
    }

    public void Stop(float fadeDuration)
    {
        if (currentSound == null) return;

        SwapSources();
        previousSound = currentSound;
        previousFade = currentFade;
        currentSound = null;
        currentFade = 0f;
        fadeSpeed = fadeDuration > 0f ? 1f / fadeDuration : float.PositiveInfinity;
        ApplyVolumes();
    }

    public void SetBusVolume(float volume)
    {
        busVolume = volume;
        ApplyVolumes();
    }

    public void Tick(float deltaTime)
    {
        float step = float.IsPositiveInfinity(fadeSpeed) ? 1f : fadeSpeed * deltaTime;

        if (currentSound != null && currentFade < 1f)
            currentFade = Mathf.Min(1f, currentFade + step);

        if (previousSound != null)
        {
            previousFade = Mathf.Max(0f, previousFade - step);
            if (previousFade <= 0f)
            {
                previous.Stop();
                previous.clip = null;
                previousSound = null;
            }
        }

        ApplyVolumes();
    }

    private void SwapSources()
    {
        AudioSource temp = current;
        current = previous;
        previous = temp;

        current.Stop();
    }

    private void ApplyVolumes()
    {
        current.volume = currentSound != null ? currentSound.Volume * currentFade * busVolume : 0f;
        previous.volume = previousSound != null ? previousSound.Volume * previousFade * busVolume : 0f;
    }

    private static void Configure(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.loop = true;
        source.volume = 0f;
    }
}
