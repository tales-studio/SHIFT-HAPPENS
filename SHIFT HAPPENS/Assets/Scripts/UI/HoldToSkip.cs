using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class HoldToSkip : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private InputActionReference skipAction;

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
    }

    public void Show(float duration, Action skip, Func<bool> blocked = null)
    {
        EnsureReady();
        holdDuration = Mathf.Max(0f, duration);
        onSkip = skip;
        isBlocked = blocked;
        held = 0f;
        active = true;

        SetProgress(0f);
        group.alpha = 1f;
        skipAction.action.Enable();
    }

    public void Hide()
    {
        EnsureReady();
        active = false;
        onSkip = null;
        isBlocked = null;
        held = 0f;

        SetProgress(0f);
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
            SetProgress(0f);
            return;
        }

        bool pressed = skipAction.action.IsPressed() || IsPointerHeld();

        held = pressed ? held + Time.unscaledDeltaTime : 0f;
        SetProgress(holdDuration > 0f ? Mathf.Clamp01(held / holdDuration) : 0f);

        if (!pressed || held < holdDuration) return;

        Action skip = onSkip;
        Hide();
        skip?.Invoke();
    }

    private bool IsPointerHeld()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.isPressed) return false;

        return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, mouse.position.ReadValue(), null);
    }

    private void SetProgress(float progress)
    {
        RectTransform rect = fill.rectTransform;
        rect.anchorMax = new Vector2(progress, rect.anchorMax.y);
    }
}
