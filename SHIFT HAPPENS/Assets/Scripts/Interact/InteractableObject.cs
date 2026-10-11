using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Configuración de Interacción")]
    [SerializeField] private GameObject readyToInteractSprite;
    [SerializeField] private float detectionRange = 3f;

    private SpriteOutlineEffect outlineEffect;
    private Transform playerTransform;
    private bool isPlayerInRange = false;

    private void Start()
    {
        outlineEffect = GetComponentInChildren<SpriteOutlineEffect>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null || outlineEffect == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= detectionRange;

        if (inRange != isPlayerInRange)
        {
            isPlayerInRange = inRange;
            outlineEffect.SetOutlineActive(isPlayerInRange);
        }
    }

    public void Interact()
    {
        if (readyToInteractSprite != null)
        {
            readyToInteractSprite.SetActive(false);
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}