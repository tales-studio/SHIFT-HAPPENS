using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerFootsteps : FootstepEmitter
{
    [SerializeField] private StepSettings sneak = new StepSettings(SoundId.PlayerFootstepSneak, 0.6f, 0.5f);
    [SerializeField] private StepSettings walk = new StepSettings(SoundId.PlayerFootstepWalk, 0.4f, 4f);
    [SerializeField] private StepSettings run = new StepSettings(SoundId.PlayerFootstepRun, 0.28f, 8f);

    [SerializeField, Min(0f)] private float runSpeedThreshold = 6f;

    private PlayerMovement movement;
    private Rigidbody2D body;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        body = GetComponent<Rigidbody2D>();
    }

    protected override StepMode GetStepMode()
    {
        if (!movement.IsMoving) return StepMode.None;
        if (movement.IsSneaking) return StepMode.Sneak;
        if (body != null && body.linearVelocity.magnitude >= runSpeedThreshold) return StepMode.Run;
        return StepMode.Walk;
    }

    protected override StepSettings GetSettings(StepMode mode)
    {
        switch (mode)
        {
            case StepMode.Sneak: return sneak;
            case StepMode.Run: return run;
            default: return walk;
        }
    }
}
