using UnityEngine;

// Punto de acceso único al jugador
[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public PlayerMovement Movement { get; private set; }
    public FlashlightLogic Flashlight { get; private set; }

    // Percepciones del jugador
    public Vector3 Position => transform.position;
    public bool IsSneaking => Movement.IsSneaking;
    public bool IsMoving => Movement.IsMoving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Movement = GetComponent<PlayerMovement>();
        Flashlight = GetComponentInChildren<FlashlightLogic>(true);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
