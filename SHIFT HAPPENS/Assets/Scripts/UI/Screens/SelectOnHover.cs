using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class SelectOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current == null || !selectable.IsInteractable()) return;
        if (EventSystem.current.currentSelectedGameObject == gameObject) return;

        EventSystem.current.SetSelectedGameObject(gameObject, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (EventSystem.current == null) return;
        if (EventSystem.current.currentSelectedGameObject != gameObject) return;

        EventSystem.current.SetSelectedGameObject(null, eventData);
    }
}
