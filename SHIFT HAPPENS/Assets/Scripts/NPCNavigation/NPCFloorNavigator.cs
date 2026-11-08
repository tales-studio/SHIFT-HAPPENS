
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCNavigation))]
public class NPCFloorNavigator : MonoBehaviour
{
    private class FloorRouteStep
    {
        public BuildingFloor FromFloor;
        public BuildingFloor ToFloor;

        public Transform EntryPoint;
        public Transform ExitPoint;
    }

    [Header("Current Floor")]
    [SerializeField]
    private BuildingFloor currentFloor =
        BuildingFloor.GroundFloor;

    [Header("Available Connections")]
    [SerializeField] private NPCFloorConnection[] connections;

    private NPCNavigation navigation;

    private NPCDestination currentDestination;

    private Transform currentDestinationSlot;

    private List<FloorRouteStep> activeRoute =
        new List<FloorRouteStep>();

    private int activeStepIndex;

    private bool followingFloorRoute;


    public BuildingFloor CurrentFloor => currentFloor;

    public NPCDestination CurrentDestination => currentDestination;

    public Transform CurrentDestinationSlot => currentDestinationSlot;

    public bool IsMoving =>
        navigation != null &&
        (navigation.IsMoving || followingFloorRoute);

    // Eventos de movimiento.
    public event Action<NPCDestination> OnMovementStarted;
    public event Action<NPCDestination> OnDestinationReached;
    public event Action<NPCDestination> OnMovementFailed;
    public event Action<NPCDestination> OnMovementCancelled;

    // Evento que informa del cambio de planta.
    public event Action<BuildingFloor, BuildingFloor> OnFloorChanged;



    private void Awake()
    {
        navigation = GetComponent<NPCNavigation>();
    }

    private void OnEnable()
    {
        if (navigation != null)
        {
            navigation.OnDestinationReached += HandleDestinationReached;
        }
    }

    private void OnDisable()
    {
        if (navigation != null)
        {
            navigation.OnDestinationReached -= HandleDestinationReached;
        }

        // Detenemos cualquier desplazamiento y liberamos
        // las reservas del NPC.
        CancelCurrentTrip();
    }


    // Comprueba si existe una ruta completa hasta el destino.
    public bool CanReachDestination(NPCDestination destination)
    {
        if (destination == null || followingFloorRoute)
            return false;

        if (destination.Floor == currentFloor)
        {
            return navigation.CanReach(destination.Position);
        }

        if (!TryBuildFloorRoute(
            currentFloor,
            destination.Floor,
            out List<FloorRouteStep> route))
        {
            return false;
        }

        return CanTraverseRoute(route, destination.Position);
    }

    // Solicita desplazarse hasta un destino.
    public bool MoveToDestination(NPCDestination destination)
    {
        if (destination == null)
            return false;

        // Cancelamos cualquier viaje anterior.
        if (IsMoving || currentDestination != null)
        {
            CancelCurrentTrip();
        }

        // Primero reservamos una posición concreta.
        if (!destination.TryReserve(
            gameObject,
            out Transform reservedSlot))
        {
            Debug.LogWarning(
                name + ": No quedan posiciones libres en " +
                destination.DisplayName
            );

            OnMovementFailed?.Invoke(destination);
            return false;
        }

        // La posición objetivo es el slot, no el centro del destino.
        Vector3 targetPosition = reservedSlot.position;

        List<FloorRouteStep> route = null;

        // Comprobamos si necesitamos cambiar de planta.
        if (destination.Floor != currentFloor)
        {
            if (!TryBuildFloorRoute(
                currentFloor,
                destination.Floor,
                out route))
            {
                destination.Release(gameObject);
                OnMovementFailed?.Invoke(destination);
                return false;
            }

            if (!CanTraverseRoute(route, targetPosition))
            {
                destination.Release(gameObject);
                OnMovementFailed?.Invoke(destination);
                return false;
            }
        }
        else if (!navigation.CanReach(targetPosition))
        {
            destination.Release(gameObject);
            OnMovementFailed?.Invoke(destination);
            return false;
        }

        bool movementStarted;

        // Destino en la misma planta.
        if (destination.Floor == currentFloor)
        {
            movementStarted = navigation.MoveTo(targetPosition);
        }
        else
        {
            // Destino en otra planta.
            activeRoute = route;
            activeStepIndex = 0;
            followingFloorRoute = true;

            movementStarted = navigation.MoveTo(
                activeRoute[0].EntryPoint.position
            );

            if (!movementStarted)
                ClearRoute();
        }

        if (!movementStarted)
        {
            destination.Release(gameObject);
            OnMovementFailed?.Invoke(destination);
            return false;
        }

        currentDestination = destination;
        currentDestinationSlot = reservedSlot;

        Debug.Log(
            name + ": Reservado " + reservedSlot.name +
            " en " + destination.DisplayName
        );

        OnMovementStarted?.Invoke(destination);

        return true;
    }



    // BFS: busca una secuencia de conexiones entre las plantas.
    private bool TryBuildFloorRoute(
        BuildingFloor startFloor,
        BuildingFloor targetFloor,
        out List<FloorRouteStep> route)
    {
        route = new List<FloorRouteStep>();

        if (startFloor == targetFloor)
            return true;

        Queue<BuildingFloor> queue =
            new Queue<BuildingFloor>();

        HashSet<BuildingFloor> visited =
            new HashSet<BuildingFloor>();

        Dictionary<BuildingFloor, BuildingFloor> previousFloor =
            new Dictionary<BuildingFloor, BuildingFloor>();

        Dictionary<BuildingFloor, NPCFloorConnection> previousConnection =
            new Dictionary<BuildingFloor, NPCFloorConnection>();

        queue.Enqueue(startFloor);
        visited.Add(startFloor);

        bool found = false;

        while (queue.Count > 0)
        {
            BuildingFloor current = queue.Dequeue();

            foreach (NPCFloorConnection connection in connections)
            {
                if (connection == null)
                    continue;

                if (!connection.TryGetNextFloor(
                    current,
                    out BuildingFloor next,
                    out Transform entry,
                    out Transform exit))
                {
                    continue;
                }

                if (visited.Contains(next))
                    continue;

                visited.Add(next);

                previousFloor[next] = current;
                previousConnection[next] = connection;

                if (next == targetFloor)
                {
                    found = true;
                    break;
                }

                queue.Enqueue(next);
            }

            if (found)
                break;
        }

        if (!found)
            return false;

        // Reconstruimos la ruta desde el destino hasta el origen.
        BuildingFloor cursor = targetFloor;

        while (cursor != startFloor)
        {
            BuildingFloor previous = previousFloor[cursor];

            NPCFloorConnection connection =
                previousConnection[cursor];

            if (!connection.TryGetPoints(
                previous,
                cursor,
                out Transform entry,
                out Transform exit))
            {
                route.Clear();
                return false;
            }

            FloorRouteStep step = new FloorRouteStep
            {
                FromFloor = previous,
                ToFloor = cursor,
                EntryPoint = entry,
                ExitPoint = exit
            };

            route.Insert(0, step);

            cursor = previous;
        }

        return true;
    }

    // Comprueba que todos los tramos de la ruta sean transitables.
    private bool CanTraverseRoute(
        List<FloorRouteStep> route,
        Vector3 finalDestination)
    {
        Vector3 segmentStart = transform.position;

        foreach (FloorRouteStep step in route)
        {
            if (step.EntryPoint == null || step.ExitPoint == null)
                return false;

            // Comprueba el trayecto hasta la siguiente escalera.
            if (!navigation.CanReachFrom(
                segmentStart,
                step.EntryPoint.position))
            {
                return false;
            }

            // El siguiente tramo comienza tras el teletransporte.
            segmentStart = step.ExitPoint.position;
        }

        // Comprueba el último trayecto hasta el destino definitivo.
        return navigation.CanReachFrom(
            segmentStart,
            finalDestination
        );
    }

    // Se ejecuta cuando NPCNavigation informa de que ha llegado.
    private void HandleDestinationReached()
    {
        if (followingFloorRoute)
        {
            ExecuteNextTransition();
            return;
        }

        // Llegada al destino definitivo.
        if (currentDestination != null)
        {
            Debug.Log(
                name + ": Ha llegado a " +
                currentDestination.DisplayName
            );

            OnDestinationReached?.Invoke(currentDestination);
        }
    }

    // Ejecuta el teletransporte y continúa la ruta.
    private void ExecuteNextTransition()
    {
        if (activeStepIndex >= activeRoute.Count)
        {
            FailCurrentTrip();
            return;
        }

        FloorRouteStep step = activeRoute[activeStepIndex];

        // Teletransportamos al NPC al otro extremo de las escaleras.
        if (!navigation.WarpTo(step.ExitPoint.position))
        {
            Debug.LogError(
                name + ": Ha fallado el teletransporte."
            );

            FailCurrentTrip();
            return;
        }

        BuildingFloor previousFloor = currentFloor;

        currentFloor = step.ToFloor;

        Debug.Log(
            name + ": Ha cambiado de " +
            previousFloor + " a " + currentFloor
        );

        OnFloorChanged?.Invoke(previousFloor, currentFloor);

        activeStepIndex++;

        // ¿Quedan más escaleras por recorrer?
        if (activeStepIndex < activeRoute.Count)
        {
            FloorRouteStep nextStep = activeRoute[activeStepIndex];

            if (!navigation.MoveTo(nextStep.EntryPoint.position))
            {
                Debug.LogError(
                    name + ": No puede llegar a la siguiente escalera."
                );

                CancelCurrentTrip();
            }

            return;
        }

        // Ya estamos en la planta de destino.
        followingFloorRoute = false;
        activeRoute.Clear();
        activeStepIndex = 0;

        // Continuamos hacia el destino definitivo.
        if (currentDestinationSlot == null ||
            !navigation.MoveTo(currentDestinationSlot.position))
        {
            Debug.LogError(
                name + ": No puede llegar al destino definitivo."
            );

            CancelCurrentTrip();
        }
    }

    private void ClearRoute()
    {
        followingFloorRoute = false;
        activeRoute.Clear();
        activeStepIndex = 0;
    }

    // Cancela el viaje completo y libera el destino.


    public void CancelCurrentTrip()
    {
        bool wasMoving = IsMoving;

        NPCDestination cancelledDestination = currentDestination;

        // Detenemos el agente.
        if (navigation != null)
        {
            navigation.StopMoving();
        }

        // Cancelamos las transiciones pendientes.
        ClearRoute();

        // Liberamos la reserva.
        ReleaseCurrentDestination();

        if (wasMoving && cancelledDestination != null)
        {
            OnMovementCancelled?.Invoke(cancelledDestination);
        }
    }



    // Libera la reserva al terminar la actividad.
    public void ReleaseCurrentDestination()
    {
        if (currentDestination != null)
        {
            currentDestination.Release(gameObject);
        }

        currentDestination = null;
        currentDestinationSlot = null;
    }

    private void FailCurrentTrip()
    {
        NPCDestination failedDestination = currentDestination;

        navigation.StopMoving();

        ClearRoute();

        ReleaseCurrentDestination();

        if (failedDestination != null)
        {
            Debug.LogWarning(
                name + ": El desplazamiento ha fallado."
            );

            OnMovementFailed?.Invoke(failedDestination);
        }
    }
}
