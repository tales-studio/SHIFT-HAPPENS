using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public class ActionPromptLabel : MonoBehaviour
{
    [SerializeField] private InputActionReference action;
    [SerializeField] private string format = "{0}";

    private TMP_Text label;
    private InputPrompts prompts;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        prompts = InputPrompts.Instance;
        if (prompts != null) prompts.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (prompts != null) prompts.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (prompts == null) return;

        label.text = string.Format(format, prompts.GetLabel(action));
    }
}
