
using System.Collections.Generic;
using UnityEngine;

public class NPCDestinationManager : MonoBehaviour
{
    public static NPCDestinationManager Instance { get; private set; }

    [Header("Available Destinations")]
    [SerializeField] private NPCDestination[] destinations;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool HasAvailableDestination(NPCDestinationType type, BuildingFloor targetFloor, GameObject requester)
    {
        foreach (NPCDestination destination in destinations)
        {
            if (destination != null &&
                destination.Type == type &&
                destination.Floor == targetFloor &&
                destination.CanReserve(requester))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryRequestRandomDestination(
        NPCDestinationType type,
        BuildingFloor targetFloor,
        NPCFloorNavigator navigator,
        out NPCDestination selectedDestination)
    {
        selectedDestination = null;

        if (navigator == null)
            return false;

        List<NPCDestination> candidates =
            new List<NPCDestination>();

        foreach (NPCDestination destination in destinations)
        {
            if (destination == null)
                continue;

            if (destination.Type != type ||
                destination.Floor != targetFloor)
            {
                continue;
            }

            // Solo consideramos destinos disponibles.
            if (destination.CanReserve(navigator.gameObject))
            {
                candidates.Add(destination);
            }
        }

        // Probamos los candidatos en orden aleatorio.
        while (candidates.Count > 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            NPCDestination chosen = candidates[randomIndex];

            candidates.RemoveAt(randomIndex);

            // El propio navegador comprueba la ruta y reserva.
            if (navigator.MoveToDestination(chosen))
            {
                selectedDestination = chosen;
                return true;
            }
        }

        Debug.LogWarning(
            "No hay destinos accesibles y disponibles de tipo " +
            type + " en " + targetFloor
        );

        return false;
    }
}
