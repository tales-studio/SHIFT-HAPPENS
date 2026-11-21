using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIScreen : MonoBehaviour
{
    [SerializeField] private Selectable firstSelected;

    private GameObject lastSelected;

    public virtual void Show(bool selectDefault)
    {
        gameObject.SetActive(true);
        if (selectDefault) SelectDefault();
    }

    public virtual void Hide()
    {
        EventSystem eventSystem = EventSystem.current;
        GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;

        if (selected != null && selected.transform.IsChildOf(transform))
        {
            lastSelected = selected;
            eventSystem.SetSelectedGameObject(null);
        }

        gameObject.SetActive(false);
    }

    public void ForgetSelection()
    {
        lastSelected = null;
    }

    public void SelectDefault()
    {
        GameObject target = lastSelected;
        if (target == null && firstSelected != null) target = firstSelected.gameObject;

        if (EventSystem.current != null && target != null)
            EventSystem.current.SetSelectedGameObject(target);
    }
}
