using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(InputDeviceTracker))]
public class InputPrompts : MonoBehaviour
{
    public static InputPrompts Instance { get; private set; }

    [SerializeField] private InputActionAsset actions;
    [SerializeField] private InputPromptLabels labels;

    private InputDeviceTracker tracker;

    public InputDeviceKind Device => tracker != null ? tracker.Current : InputDeviceKind.KeyboardMouse;

    public event Action Changed;

    private void Awake()
    {
        Instance = this;
        tracker = GetComponent<InputDeviceTracker>();
    }

    private void OnEnable()
    {
        tracker.Changed += HandleDeviceChanged;
    }

    private void OnDisable()
    {
        tracker.Changed -= HandleDeviceChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public string GetLabel(InputActionReference reference)
    {
        if (reference == null || reference.action == null) return string.Empty;

        return GetLabel(reference.action);
    }

    public string GetLabel(InputAction action)
    {
        if (labels == null) return action.GetBindingDisplayString();

        return labels.GetLabel(action, Device);
    }

    public bool TryGetLabel(string actionName, out string label)
    {
        InputAction action = actions != null ? actions.FindAction(actionName) : null;
        label = action != null ? GetLabel(action) : null;

        return action != null;
    }

    private void HandleDeviceChanged(InputDeviceKind device)
    {
        Changed?.Invoke();
    }
}
