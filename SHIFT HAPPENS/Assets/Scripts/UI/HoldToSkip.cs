using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class HoldToSkip : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text label;
    [SerializeField] private string labelFormat = "Hold {0} to skip";
    [SerializeField] private InputAction skipAction = new InputAction("Skip", InputActionType.Button);

    private CanvasGroup group;
    private Action onSkip;
    private Func<bool> isBlocked;
    private float holdDuration;
    private float held;
    private bool active;

    private void Awake()
    {
        if (!active) Hide();
    }

    private void EnsureReady()
    {
        if (group != null) return;

        group = GetComponent<CanvasGroup>();

        if (skipAction.bindings.Count == 0)
        {
            skipAction.AddBinding("<Keyboard>/x");
            skipAction.AddBinding("<Gamepad>/select");
        }
    }

    private void OnDestroy()
    {
        skipAction.Disable();
    }

    public void Show(float duration, Action skip, Func<bool> blocked = null)
    {
        EnsureReady();
        holdDuration = Mathf.Max(0f, duration);
        onSkip = skip;
        isBlocked = blocked;
        held = 0f;
        active = true;

        label.text = string.Format(labelFormat, skipAction.GetBindingDisplayString(0));
        fill.fillAmount = 0f;
        group.alpha = 1f;
        skipAction.Enable();
    }

    public void Hide()
    {
        EnsureReady();
        active = false;
        onSkip = null;
        isBlocked = null;
        held = 0f;

        skipAction.Disable();
        fill.fillAmount = 0f;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    private void Update()
    {
        if (!active) return;

        if (isBlocked != null && isBlocked())
        {
            held = 0f;
            fill.fillAmount = 0f;
            return;
        }

        held = skipAction.IsPressed() ? held + Time.unscaledDeltaTime : 0f;
        fill.fillAmount = holdDuration > 0f ? Mathf.Clamp01(held / holdDuration) : 0f;

        if (!skipAction.IsPressed() || held < holdDuration) return;

        Action skip = onSkip;
        Hide();
        skip?.Invoke();
    }
}
