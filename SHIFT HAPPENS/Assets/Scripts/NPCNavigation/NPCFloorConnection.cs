
using UnityEngine;

public class NPCFloorConnection : MonoBehaviour
{
    [Header("First Floor")]
    [SerializeField] private BuildingFloor floorA;
    [SerializeField] private Transform pointA;

    [Header("Second Floor")]
    [SerializeField] private BuildingFloor floorB;
    [SerializeField] private Transform pointB;

    [Header("Settings")]
    [SerializeField] private bool bidirectional = true;

    public BuildingFloor FloorA => floorA;
    public BuildingFloor FloorB => floorB;

    public Transform PointA => pointA;
    public Transform PointB => pointB;

    public bool Bidirectional => bidirectional;

    // Obtiene los puntos de entrada y salida
    // según la planta de origen y destino.
    public bool TryGetPoints(
        BuildingFloor currentFloor,
        BuildingFloor targetFloor,
        out Transform entryPoint,
        out Transform exitPoint)
    {
        entryPoint = null;
        exitPoint = null;

        // Trayecto A -> B.
        if (currentFloor == floorA && targetFloor == floorB)
        {
            entryPoint = pointA;
            exitPoint = pointB;

            return true;
        }

        // Trayecto B -> A.
        if (bidirectional &&
            currentFloor == floorB &&
            targetFloor == floorA)
        {
            entryPoint = pointB;
            exitPoint = pointA;

            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (pointA == null || pointB == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(pointA.position, 0.35f);
        Gizmos.DrawWireSphere(pointB.position, 0.35f);

        Gizmos.DrawLine(pointA.position, pointB.position);
    }
    public bool TryGetNextFloor(
        BuildingFloor fromFloor,
        out BuildingFloor nextFloor,
        out Transform entryPoint,
        out Transform exitPoint)
    {
        nextFloor = default;
        entryPoint = null;
        exitPoint = null;

        // Dirección A -> B.
        if (fromFloor == floorA)
        {
            nextFloor = floorB;
            entryPoint = pointA;
            exitPoint = pointB;

            return entryPoint != null && exitPoint != null;
        }

        // Dirección B -> A.
        if (bidirectional && fromFloor == floorB)
        {
            nextFloor = floorA;
            entryPoint = pointB;
            exitPoint = pointA;

            return entryPoint != null && exitPoint != null;
        }

        return false;
    }

}
