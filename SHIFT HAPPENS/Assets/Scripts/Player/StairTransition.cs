
using UnityEngine;

public class StairTransition : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Collider2D playerStairCollider;

    [Header("Destination")]
    [SerializeField] private Transform destination;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != playerStairCollider) return;

        Rigidbody2D playerBody = playerStairCollider.attachedRigidbody;

        if (playerBody == null || destination == null)
            return;

        if (FadeManager.Instance == null)
        {
            Debug.LogError("FadeManager no encontrado.");
            return;
        }

        FadeManager.Instance.Transition(
            playerBody,
            destination
        );
    }
}
