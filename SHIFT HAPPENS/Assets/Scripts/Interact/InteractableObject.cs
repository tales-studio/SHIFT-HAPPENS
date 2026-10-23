using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    //Solo ofrece funciones para activar y desactivar el objeto
    //"listo para interactuar" y avisa de que la interacción se ha producido

    [SerializeField] private GameObject readyToInteractObj;

    public UnityEvent onInteract;

    public void Interact()
    {
        onInteract.Invoke();
    }

    public void ShowIndicators()
    {
        if (readyToInteractObj != null)
        {
            readyToInteractObj.SetActive(true);
        }
    
    }

    public void HideIndicators()
    {
        if (readyToInteractObj != null)
        {
            readyToInteractObj.SetActive(false);
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}