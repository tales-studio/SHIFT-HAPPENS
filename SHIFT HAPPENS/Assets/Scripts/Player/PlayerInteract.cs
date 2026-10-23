using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private List<IInteractable> interactablesInRange;

    private void Awake()
    {
        interactablesInRange = new List<IInteractable>(); 
    }

    private void OnTriggerEnter2D(Collider2D foundCollider)
    {
        IInteractable interactable = foundCollider.GetComponentInParent<IInteractable>();

        if (interactable != null && !interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Add(interactable); 
        }
    }

    private void OnTriggerExit2D(Collider2D foundCollider)
    {
        IInteractable interactable = foundCollider.GetComponentInParent<IInteractable>(); 

        if (interactable != null)
        {
            interactablesInRange.Remove(interactable); 
        }
    }

    public void OnInteract()
    {
        if (interactablesInRange.Count > 0)
        {
            IInteractable closest = GetClosestInteractable();
           
            if (closest != null)
            {
                closest.Interact(); 
            }
        }
    }

    private IInteractable GetClosestInteractable()
    {
        IInteractable closest = null;
        float minDistance = float.MaxValue; 

        foreach (IInteractable interactable in interactablesInRange)
        {
            if (interactable == null || interactable.GetTransform() == null) continue; 

            float distance = Vector2.Distance(transform.position, interactable.GetTransform().position); 

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = interactable; 
            }
        }
        return closest;
    }

    private void OnDisable()
    {
        interactablesInRange.Clear(); 
    }
}