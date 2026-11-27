using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class InputDeviceTracker : MonoBehaviour
{
    public static InputDeviceTracker Instance { get; private set; }

    public InputDeviceKind Current { get; private set; } = InputDeviceKind.KeyboardMouse;

    public event Action<InputDeviceKind> Changed;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += HandleActionChange;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= HandleActionChange;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void HandleActionChange(object target, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;
        if (!(target is InputAction action)) return;

        InputControl control = action.activeControl;
        if (control == null) return;

        SetCurrent(control.device is Gamepad ? InputDeviceKind.Gamepad : InputDeviceKind.KeyboardMouse);
    }

    private void SetCurrent(InputDeviceKind kind)
    {
        if (kind == Current) return;

        Current = kind;
        Changed?.Invoke(kind);
    }
}
