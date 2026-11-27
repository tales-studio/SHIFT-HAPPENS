using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PromptLabels", menuName = "Shift Happens/Input Prompt Labels")]
public class InputPromptLabels : ScriptableObject
{
    private const string KeyboardMouseGroup = "Keyboard&Mouse";
    private const string GamepadGroup = "Gamepad";

    [Serializable]
    public class Entry
    {
        public InputActionReference action;
        public string keyboardLabel;
        public string gamepadLabel;
        public string webKeyboardLabel;
    }

    [SerializeField] private Entry[] entries = new Entry[0];

    public string GetLabel(InputAction action, InputDeviceKind device)
    {
        Entry entry = Find(action);
        string label = entry != null ? Pick(entry, device) : null;

        return string.IsNullOrEmpty(label) ? FromBindings(action, device) : label;
    }

    private Entry Find(InputAction action)
    {
        foreach (Entry entry in entries)
        {
            if (entry.action != null && entry.action.action != null && entry.action.action.id == action.id)
                return entry;
        }

        return null;
    }

    private static string Pick(Entry entry, InputDeviceKind device)
    {
        if (device == InputDeviceKind.Gamepad) return entry.gamepadLabel;

#if UNITY_WEBGL
        if (!string.IsNullOrEmpty(entry.webKeyboardLabel)) return entry.webKeyboardLabel;
#endif
        return entry.keyboardLabel;
    }

    private static string FromBindings(InputAction action, InputDeviceKind device)
    {
        string group = device == InputDeviceKind.Gamepad ? GamepadGroup : KeyboardMouseGroup;
        return action.GetBindingDisplayString(InputBinding.MaskByGroup(group));
    }
}
