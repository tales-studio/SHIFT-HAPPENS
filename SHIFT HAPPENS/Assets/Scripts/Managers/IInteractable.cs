using UnityEngine;

public interface IInteractable
{
    public bool CanInteract { get; }
    public Vector3 SignPosition { get; }
    public void Interact();
    public Transform GetTransform();
}
