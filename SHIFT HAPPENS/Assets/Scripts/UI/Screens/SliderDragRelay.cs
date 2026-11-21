using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderDragRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action Pressed;
    public event Action Released;

    public bool IsHeld { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        IsHeld = true;
        Pressed?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsHeld || eventData.button != PointerEventData.InputButton.Left) return;
        Release();
    }

    private void OnDisable()
    {
        if (IsHeld) Release();
    }

    private void Release()
    {
        IsHeld = false;
        Released?.Invoke();
    }
}
