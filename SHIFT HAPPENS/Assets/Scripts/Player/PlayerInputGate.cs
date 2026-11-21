using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputGate : MonoBehaviour
{
    private PlayerInput input;
    private GameManager gameManager;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null) return;

        gameManager.OnPaused += Block;
        gameManager.OnResumed += HandleResumed;
        gameManager.OnShiftEnded += HandleShiftEnded;
    }

    private void OnDestroy()
    {
        if (gameManager == null) return;

        gameManager.OnPaused -= Block;
        gameManager.OnResumed -= HandleResumed;
        gameManager.OnShiftEnded -= HandleShiftEnded;
    }

    private void HandleResumed()
    {
        if (gameManager.isPlaying) input.ActivateInput();
    }

    private void HandleShiftEnded(ShiftResult result)
    {
        Block();
    }

    private void Block()
    {
        input.DeactivateInput();
    }
}
