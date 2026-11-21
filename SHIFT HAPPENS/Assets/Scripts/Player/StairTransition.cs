
using UnityEngine;

public class StairTransition : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Collider2D playerStairCollider;

    [Header("Destination")]
    [SerializeField] private Transform destination;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != playerStairCollider || destination == null) return;

        PlayerTeleporter teleporter = other.GetComponentInParent<PlayerTeleporter>();

        if (teleporter == null)
        {
            Debug.LogError("PlayerTeleporter no encontrado.", this);
            return;
        }

        teleporter.Teleport(destination);
    }
}
