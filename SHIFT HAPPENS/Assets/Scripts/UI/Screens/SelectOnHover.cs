using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class SelectOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    private Selectable selectable;
    private bool hovered;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    private void OnDisable()
    {
        hovered = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        if (EventSystem.current == null || !selectable.IsInteractable()) return;
        if (EventSystem.current.currentSelectedGameObject == gameObject) return;

        EventSystem.current.SetSelectedGameObject(gameObject, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        if (eventData.pointerPress == gameObject) return;
        Deselect(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!hovered) Deselect(eventData);
    }

    private void Deselect(PointerEventData eventData)
    {
        if (EventSystem.current == null) return;
        if (EventSystem.current.currentSelectedGameObject != gameObject) return;

        EventSystem.current.SetSelectedGameObject(null, eventData);
    }
}
