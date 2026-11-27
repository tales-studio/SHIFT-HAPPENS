using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sneakAction;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference aimAction;
    [SerializeField] private InputActionReference aimStickAction;

    private PlayerInput playerInput;
    private InputAction move;
    private InputAction sneak;
    private InputAction interact;
    private InputAction aim;
    private InputAction aimStick;

    public event Action Interacted;

    public Vector2 Move => move.ReadValue<Vector2>();
    public bool IsSneakHeld => sneak.IsPressed();
    public Vector2 AimScreenPosition => aim.ReadValue<Vector2>();
    public Vector2 AimStick => aimStick.ReadValue<Vector2>();
    public bool IsAimActive => aim.enabled;
    public InputAction InteractAction => interact;

    public bool UsingGamepad
    {
        get
        {
            if (playerInput == null) return false;

            foreach (InputDevice device in playerInput.devices)
                if (device is Gamepad) return true;

            return false;
        }
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        move = Resolve(moveAction);
        sneak = Resolve(sneakAction);
        interact = Resolve(interactAction);
        aim = Resolve(aimAction);
        aimStick = Resolve(aimStickAction);
    }

    private void Start()
    {
        if (InputContextService.Instance != null) InputContextService.Instance.Refresh();
    }

    private void OnEnable()
    {
        interact.performed += HandleInteract;
    }

    private void OnDisable()
    {
        interact.performed -= HandleInteract;
    }

    private InputAction Resolve(InputActionReference reference)
    {
        if (playerInput == null || playerInput.actions == null) return reference.action;

        return playerInput.actions.FindAction(reference.action.id) ?? reference.action;
    }

    private void HandleInteract(InputAction.CallbackContext context)
    {
        Interacted?.Invoke();
    }
}
