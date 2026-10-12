using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Configuración de Interacción")]
    [SerializeField] private GameObject readyToInteractSprite;
    [SerializeField] private float detectionRange = 3f;

    private Transform playerTransform;
    private bool isPlayerInRange = false;
    private HighlightVFX highlightVFX;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (transform.parent != null)
        {
            highlightVFX = transform.parent.GetComponentInChildren<HighlightVFX>();
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= detectionRange;

        if (inRange != isPlayerInRange)
        {
            isPlayerInRange = inRange;
        }
    }

    public void Interact()
    {
        if (readyToInteractSprite != null)
        {
            readyToInteractSprite.SetActive(false);
        }

        if (highlightVFX != null)
        {
            highlightVFX.DisableHighlight();
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}