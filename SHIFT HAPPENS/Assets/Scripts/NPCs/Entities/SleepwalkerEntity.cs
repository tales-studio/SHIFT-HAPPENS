using BehaviourAPI.Core;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SleepwalkerEntity : EntityBrain
{
    [Header("Sonidos")]
    [SerializeField] private AudioClip screamClip;
    [SerializeField] private AudioClip vanishClip;

    private AudioSource audioSource;
    private float sneakTimer;
    private bool vanishedBySneaking;

    public override bool UsesWatchState => true;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
    }

    // Aparecer: el grito. Su duración (appearDuration) es el margen para agacharse.
    public override void StartAppear()
    {
        base.StartAppear();
        Play(screamClip);
    }

    public override void StartWatch()
    {
        sneakTimer = 0f;
    }

    // Percepción pull "Ramón de pie" (no agachado)
    public override bool ShouldStartChase()
    {
        return TargetPlayer == null || !TargetPlayer.IsSneaking;
    }

    // Cuenta el tiempo que Ramón aguanta agachado
    public override Status UpdateWatch()
    {
        if (TargetPlayer != null && TargetPlayer.IsSneaking)
            sneakTimer += Time.deltaTime;

        if (sneakTimer >= Config.sneakTimeToVanish)
        {
            vanishedBySneaking = true;
            return Status.Success;
        }
        return Status.Running;
    }

    public override void StartDisappear()
    {
        base.StartDisappear();
        if (vanishedBySneaking) Play(vanishClip);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.Stop();
        audioSource.PlayOneShot(clip);
    }
}
