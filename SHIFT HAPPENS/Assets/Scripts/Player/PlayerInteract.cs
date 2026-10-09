using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private List<IInteractable> interactablesInRange;
    private IHighlightable _currentHighlighted;

    private void Awake()
    {
        interactablesInRange = new List<IInteractable>();
    }

    private void Update()
    {
        UpdateHighlighting();
    }

    private void OnTriggerEnter2D(Collider2D foundCollider)
    {
        //El collider de interacción es un objeto hijo que se activa y desactiva, 
        //Por lo tanto, tenemos que buscar el IInteractable en su padre, que es quien lo gestiona todo
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
        if(interactablesInRange.Count > 0)
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
        //Iniciamos la distancia a comparar a la máxima para que el primer
        //objeto que chequeamos siempre sea el más cercano
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

    private void UpdateHighlighting()
    {
        interactablesInRange.RemoveAll(item => item == null || item.GetTransform() == null);

        IInteractable closestInteractable = GetClosestInteractable();
        IHighlightable newHighlightable = null;

        if (closestInteractable != null)
        {
            if (closestInteractable is IHighlightable highlightable)
            {
                newHighlightable = highlightable;
            }
            else
            {
                newHighlightable = closestInteractable.GetTransform().GetComponent<IHighlightable>();
            }
        }

        if (_currentHighlighted != newHighlightable)
        {
            _currentHighlighted?.SetHighlight(false);
            _currentHighlighted = newHighlightable;
            _currentHighlighted?.SetHighlight(true);
        }
    }

    private void OnDisable()
    {
        _currentHighlighted?.SetHighlight(false);
        _currentHighlighted = null;
        interactablesInRange.Clear();
    }
}
