
using System.Collections.Generic;
using UnityEngine;

public class NPCDestinationManager : MonoBehaviour
{
    [Header("Available Destinations")]
    [SerializeField] private NPCDestination[] destinations;

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
