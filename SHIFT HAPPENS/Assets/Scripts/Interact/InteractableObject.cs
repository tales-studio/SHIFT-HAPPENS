using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject readyToInteractSprite;
    public void Interact()
    {
        //Desactivar el sprite reventado
        readyToInteractSprite.SetActive(false);

        //Notificar al gestor de tareas
    }


    public Transform GetTransform()
    {
        return transform;
    }

}
