
using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCNavigation : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField, Min(0.01f)]
    private float destinationSampleRadius = 0.5f;

    private NavMeshAgent agent;
    private bool hasDestination;

    public bool IsMoving => hasDestination && agent != null;

    public NavMeshPathStatus LastPathStatus { get; private set; }
        = NavMeshPathStatus.PathInvalid;

    public event Action OnDestinationReached;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.baseOffset = 0f;
    }

    private void Start()
    {
        if (NavMesh.SamplePosition(
            transform.position,
            out NavMeshHit hit,
            2f,
            agent.areaMask))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogError(
                "No se ha encontrado un NavMesh válido para " + name
            );
        }
    }

    private void Update()
    {
        if (!hasDestination || !agent.isOnNavMesh || agent.pathPending)
            return;

        // Si la ruta deja de ser completa, cancelamos el desplazamiento.
        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            hasDestination = false;
            Debug.LogWarning("La ruta de " + name + " ha dejado de ser válida.");
            return;
        }

        float stoppingThreshold = Mathf.Max(
            agent.stoppingDistance,
            0.05f
        );

        if (agent.remainingDistance <= stoppingThreshold &&
            agent.velocity.sqrMagnitude < 0.01f)
        {
            hasDestination = false;
            agent.ResetPath();

            OnDestinationReached?.Invoke();
        }
    }

    // Comprueba si existe una ruta completa sin mover al NPC.
    public bool CanReach(Vector3 destination)
    {
        return TryCalculateCompletePath(destination, out _);
    }

    // Calcula una ruta completa hasta el destino solicitado.
    private bool TryCalculateCompletePath(
        Vector3 destination,
        out NavMeshPath path)
    {
        path = new NavMeshPath();
        LastPathStatus = NavMeshPathStatus.PathInvalid;

        if (agent == null || !agent.isOnNavMesh)
            return false;

        // Buscamos una posición válida del NavMesh cercana al destino.
        if (!NavMesh.SamplePosition(
            destination,
            out NavMeshHit hit,
            destinationSampleRadius,
            agent.areaMask))
        {
            return false;
        }

        // Calculamos la ruta sin iniciar el movimiento.
        bool pathFound = agent.CalculatePath(hit.position, path);

        LastPathStatus = path.status;

        return pathFound &&
               path.status == NavMeshPathStatus.PathComplete;
    }

    // Calcula y comienza el desplazamiento.
    public bool MoveTo(Vector3 destination)
    {
        if (!TryCalculateCompletePath(destination, out NavMeshPath path))
        {
            Debug.LogWarning(
                name + " no puede llegar al destino. Estado: " +
                LastPathStatus
            );

            return false;
        }

        agent.isStopped = false;

        hasDestination = agent.SetPath(path);

        if (!hasDestination)
        {
            Debug.LogWarning(
                "No se ha podido asignar la ruta a " + name
            );
        }

        return hasDestination;
    }

    public void StopMoving()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();

        hasDestination = false;
    }

    // Teletransporta al NPC a otra zona del NavMesh.
    public bool WarpTo(Vector3 destination)
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning(name + ": El agente no está sobre el NavMesh.");
            return false;
        }

        // Cancelamos cualquier desplazamiento anterior.
        agent.isStopped = true;
        agent.ResetPath();
        hasDestination = false;

        // Buscamos una posición válida cerca del punto de salida.
        if (!NavMesh.SamplePosition(
            destination,
            out NavMeshHit hit,
            destinationSampleRadius,
            agent.areaMask))
        {
            Debug.LogError(
                name + ": No se ha encontrado NavMesh en el destino del teletransporte."
            );

            agent.isStopped = false;
            return false;
        }

        // Teletransportamos el agente.
        bool success = agent.Warp(hit.position);

        agent.isStopped = false;

        if (!success)
        {
            Debug.LogError(name + ": El teletransporte ha fallado.");
            return false;
        }

        return true;
    }


    public bool CanReachFrom(Vector3 start, Vector3 destination)
    {
        if (agent == null)
            return false;

        // Utilizamos el mismo tipo de agente y áreas
        // que tiene configurado nuestro NPC.
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        // Comprobamos que el origen está sobre el NavMesh.
        if (!NavMesh.SamplePosition(
            start,
            out NavMeshHit startHit,
            destinationSampleRadius,
            filter))
        {
            return false;
        }

        // Comprobamos que el destino está sobre el NavMesh.
        if (!NavMesh.SamplePosition(
            destination,
            out NavMeshHit destinationHit,
            destinationSampleRadius,
            filter))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();

        bool found = NavMesh.CalculatePath(
            startHit.position,
            destinationHit.position,
            filter,
            path
        );

        LastPathStatus = path.status;

        return found &&
               path.status == NavMeshPathStatus.PathComplete;
    }

}
