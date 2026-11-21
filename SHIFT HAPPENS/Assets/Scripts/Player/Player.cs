using System;
using UnityEngine;

// Punto de acceso único al jugador
[RequireComponent(typeof(PlayerMovement), typeof(PlayerTeleporter))]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public PlayerMovement Movement { get; private set; }
    public PlayerTeleporter Teleporter { get; private set; }
    public FlashlightLogic Flashlight { get; private set; }

    // Percepciones del jugador
    public Vector3 Position => transform.position;
    public bool IsSneaking => Movement.IsSneaking;
    public bool IsMoving => Movement.IsMoving;

    // Percepción push "Escondido en la taquilla"
    public bool IsHidden { get; private set; }
    public event Action<bool> OnHiddenChanged;

    private object hiddenBy; // la taquilla que lo esconde

    // Solo puede sacarlo la misma taquilla que lo escondió
    public void SetHidden(bool hidden, object source)
    {
        if (!hidden && hiddenBy != source) return;
        hiddenBy = hidden ? source : null;

        if (IsHidden == hidden) return;
        IsHidden = hidden;
        OnHiddenChanged?.Invoke(hidden);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Movement = GetComponent<PlayerMovement>();
        Teleporter = GetComponent<PlayerTeleporter>();
        Flashlight = GetComponentInChildren<FlashlightLogic>(true);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
