using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class NpcWalkOut : MonoBehaviour
{
    [SerializeField, Min(0f)] private float arriveDistance = 0.5f;

    private NavMeshAgent agent;
    private bool leaving;

    public bool IsLeaving => leaving;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public void Leave(Vector3 exit)
    {
        if (!gameObject.activeInHierarchy) return;

        if (!agent.isOnNavMesh || !agent.SetDestination(exit))
        {
            Despawn();
            return;
        }

        leaving = true;
    }

    public void Despawn()
    {
        leaving = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!leaving || agent.pathPending) return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.remainingDistance <= arriveDistance)
            Despawn();
    }
}
