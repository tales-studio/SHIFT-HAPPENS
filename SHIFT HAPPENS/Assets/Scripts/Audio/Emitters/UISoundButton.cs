using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class UISoundButton : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IPointerClickHandler, ISubmitHandler
{
    [SerializeField] private SoundId selectSound = SoundId.UISelect;
    [SerializeField] private SoundId confirmSound = SoundId.UIConfirm;

    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    private bool CanPlay => selectable != null && selectable.IsInteractable();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanPlay) AudioServices.Sfx.Play(selectSound);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (CanPlay && !(eventData is PointerEventData)) AudioServices.Sfx.Play(selectSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CanPlay && eventData.button == PointerEventData.InputButton.Left) AudioServices.Sfx.Play(confirmSound);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (CanPlay) AudioServices.Sfx.Play(confirmSound);
    }
}
