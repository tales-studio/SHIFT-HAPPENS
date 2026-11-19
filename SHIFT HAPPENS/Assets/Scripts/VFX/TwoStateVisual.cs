using System;
using UnityEngine;

public class TwoStateVisual : MonoBehaviour
{
    [SerializeField] private bool startsInAlternateState;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Closed door / working object")]
    [SerializeField] private Sprite defaultSprite;

    [Tooltip("Open door / broken object")]
    [SerializeField] private Sprite alternateSprite;

    [Tooltip("Enabled only in the alternate state (sparks, smoke...)")]
    [SerializeField] private GameObject[] alternateOnlyObjects;

    public bool IsInAlternateState { get; private set; }

    public event Action<bool> StateChanged;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        IsInAlternateState = startsInAlternateState;
        ApplyVisuals();
    }

    public void Toggle()
    {
        SetAlternateState(!IsInAlternateState);
    }

    public void SetAlternateState(bool alternate)
    {
        if (alternate == IsInAlternateState) return;

        IsInAlternateState = alternate;
        ApplyVisuals();
        StateChanged?.Invoke(alternate);
    }

    private void ApplyVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = IsInAlternateState ? alternateSprite : defaultSprite;
        }

        if (alternateOnlyObjects == null) return;

        foreach (GameObject obj in alternateOnlyObjects)
        {
            if (obj != null) obj.SetActive(IsInAlternateState);
        }
    }
}
