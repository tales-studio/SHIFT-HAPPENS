using System;
using UnityEngine;

public enum StepMode
{
    None,
    Sneak,
    Walk,
    Run
}

public abstract class FootstepEmitter : MonoBehaviour
{
    [Serializable]
    protected struct StepSettings
    {
        public SoundId sound;
        [Min(0.05f)] public float interval;
        [Min(0f)] public float noiseRadius;
        [Range(0f, 1f)] public float volume;

        public StepSettings(SoundId sound, float interval, float noiseRadius, float volume)
        {
            this.sound = sound;
            this.interval = interval;
            this.noiseRadius = noiseRadius;
            this.volume = volume;
        }
    }

    [SerializeField] private bool emitsNoise;

    private float timer;

    protected abstract StepMode GetStepMode();
    protected abstract StepSettings GetSettings(StepMode mode);

    protected virtual void Update()
    {
        StepMode mode = GetStepMode();
        if (mode == StepMode.None)
        {
            timer = 0f;
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        StepSettings settings = GetSettings(mode);
        timer = settings.interval;

        Vector3 position = transform.position;
        FootstepSurface surface = FootstepSurfaces.Provider.GetSurfaceAt(position);

        float volume = settings.volume;
        float noiseRadius = settings.noiseRadius;
        if (surface != null)
        {
            volume *= surface.Volume;
            noiseRadius *= surface.NoiseMultiplier;
        }

        PlayStep(surface, settings.sound, position).SetVolume(volume);

        if (emitsNoise && noiseRadius > 0f)
            NoiseEvents.Emit(position, noiseRadius, gameObject);
    }

    private static SoundHandle PlayStep(FootstepSurface surface, SoundId fallback, Vector3 position)
    {
        if (surface != null && surface.Steps != null)
            return AudioServices.Sfx.PlayAt(surface.Steps, position);
        return AudioServices.Sfx.PlayAt(fallback, position);
    }
}
