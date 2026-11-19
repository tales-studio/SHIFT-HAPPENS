
using UnityEngine;

public class StairTransition : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private Transform destination;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D playerBody = other.attachedRigidbody;

        if (playerBody == null || !playerBody.CompareTag("Player"))
            return;

        if (FadeManager.Instance == null)
        {
            Debug.LogError("FadeManager no encontrado en la escena.");
            return;
        }

        FadeManager.Instance.Transition(
            playerBody,
            destination
        );
    }
}
