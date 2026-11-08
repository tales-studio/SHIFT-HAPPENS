
using System.Collections.Generic;
using UnityEngine;

public enum BuildingFloor
{
    Basement,
    GroundFloor,
    FirstFloor,
    SecondFloor
}

public enum NPCDestinationType
{
    Generic,
    Bedroom,
    Bathroom,
    Kitchen,
    StudyRoom,
    CommonRoom,
    Reception,
    Laundry
}

public class NPCDestination : MonoBehaviour
{
    [Header("Destination Information")]
    [SerializeField] private string displayName;
    [SerializeField] private NPCDestinationType destinationType;
    [SerializeField] private BuildingFloor floor;

    [Header("Destination Slots")]
    [SerializeField] private Transform[] slots;

    // Cada NPC tiene reservada una posición concreta.
    private readonly Dictionary<GameObject, Transform> reservations =
        new Dictionary<GameObject, Transform>();

    public string DisplayName => displayName;
    public NPCDestinationType Type => destinationType;
    public BuildingFloor Floor => floor;

    // Posición central del destino.
    public Vector3 Position => transform.position;

    public int CurrentOccupants
    {
        get
        {
            CleanReservations();
            return reservations.Count;
        }
    }

    public int Capacity => GetSlotCount();

    public bool IsAvailable
    {
        get
        {
            CleanReservations();

            for (int i = 0; i < GetSlotCount(); i++)
            {
                Transform slot = GetSlot(i);

                if (slot != null && !IsSlotReserved(slot))
                    return true;
            }

            return false;
        }
    }

    private int GetSlotCount()
    {
        // Si no configuramos slots, utilizamos el propio destino
        // como una única posición de ocupación.
        return slots == null || slots.Length == 0
            ? 1
            : slots.Length;
    }

    private Transform GetSlot(int index)
    {
        if (slots == null || slots.Length == 0)
            return index == 0 ? transform : null;

        return slots[index];
    }

    private bool IsSlotReserved(Transform slot)
    {
        foreach (Transform reservedSlot in reservations.Values)
        {
            if (reservedSlot == slot)
                return true;
        }

        return false;
    }

    public bool CanReserve(GameObject requester)
    {
        return GetAvailableSlot(requester) != null;
    }

    // Intenta reservar una posición concreta.
    public bool TryReserve(
    GameObject requester,
    out Transform reservedSlot)
    {
        reservedSlot = GetAvailableSlot(requester);

        if (reservedSlot == null)
            return false;

        // Si ya estaba reservado, no duplicamos la reserva.
        if (reservations.ContainsKey(requester))
            return true;

        reservations.Add(requester, reservedSlot);

        Debug.Log(
            requester.name + " ha reservado " +
            reservedSlot.name + " en " + displayName
        );

        return true;
    }

    // Mantenemos esta sobrecarga para compatibilidad
    // con las llamadas antiguas.
    public bool TryReserve(GameObject requester)
    {
        return TryReserve(requester, out _);
    }

    public Transform GetReservedSlot(GameObject requester)
    {
        if (requester == null)
            return null;

        return reservations.TryGetValue(requester, out Transform slot)
            ? slot
            : null;
    }

    public void Release(GameObject requester)
    {
        if (requester == null)
            return;

        if (reservations.Remove(requester))
        {
            Debug.Log(
                requester.name + " ha liberado una posición en " +
                displayName
            );
        }
    }

    private void CleanReservations()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (KeyValuePair<GameObject, Transform> reservation
                 in reservations)
        {
            if (reservation.Key == null || reservation.Value == null)
            {
                toRemove.Add(reservation.Key);
            }
        }

        foreach (GameObject requester in toRemove)
        {
            reservations.Remove(requester);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, 0.2f);

        if (slots == null || slots.Length == 0)
            return;

        Gizmos.color = Color.green;

        foreach (Transform slot in slots)
        {
            if (slot != null)
            {
                Gizmos.DrawWireSphere(slot.position, 0.18f);
            }
        }
    }

    public Transform GetAvailableSlot(GameObject requester)
    {
        if (requester == null)
            return null;

        CleanReservations();

        // Si ya tiene una posición reservada, devolvemos esa.
        if (reservations.TryGetValue(
            requester,
            out Transform reservedSlot))
        {
            return reservedSlot;
        }

        // Buscamos una posición libre.
        for (int i = 0; i < GetSlotCount(); i++)
        {
            Transform slot = GetSlot(i);

            if (slot != null && !IsSlotReserved(slot))
                return slot;
        }

        return null;
    }
}
