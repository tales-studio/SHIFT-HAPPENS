using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private GameObject readyToInteractSprite;
    private IHighlightable _highlightEffect;

    private void Awake()
    {
        _highlightEffect = GetComponent<ObjectOutlineHighlighter>();

        if (_highlightEffect == null)
        {
            _highlightEffect = GetComponentInChildren<ObjectOutlineHighlighter>();
        }
    }

    public void Interact()
    {
        //Desactivar el sprite reventado
        readyToInteractSprite.SetActive(false);

        SetHighlight(false);

        //Notificar al gestor de tareas
    }


    public Transform GetTransform()
    {
        return transform;
    }

    public void SetHighlight(bool active)
    {
        _highlightEffect?.SetHighlight(active);
    }
}
