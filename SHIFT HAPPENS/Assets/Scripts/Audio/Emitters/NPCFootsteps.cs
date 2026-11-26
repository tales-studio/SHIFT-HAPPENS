using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCFootsteps : FootstepEmitter
{
    [SerializeField] private StepSettings walk = new StepSettings(SoundId.NPCFootstep, 0.45f, 0f, 1f);
    [SerializeField, Min(0f)] private float minSpeed = 0.1f;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected override StepMode GetStepMode()
    {
        if (!agent.enabled || agent.velocity.sqrMagnitude < minSpeed * minSpeed) return StepMode.None;
        return StepMode.Walk;
    }

    protected override StepSettings GetSettings(StepMode mode) => walk;
}
