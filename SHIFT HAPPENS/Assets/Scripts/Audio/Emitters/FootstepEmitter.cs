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

        public StepSettings(SoundId sound, float interval, float noiseRadius)
        {
            this.sound = sound;
            this.interval = interval;
            this.noiseRadius = noiseRadius;
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

        AudioServices.Sfx.PlayAt(settings.sound, transform.position);

        if (emitsNoise && settings.noiseRadius > 0f)
            NoiseEvents.Emit(transform.position, settings.noiseRadius, gameObject);
    }
}
