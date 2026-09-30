using System;
using System.Collections.Generic;
using NUnit.Framework;
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
            
            float distance = Vector2.Distance(transform.position, interactable.GetTransform().position);

            if (closest == null)
            {
                closest = interactable;
            }
            else
            {
                if(distance < minDistance)
                {
                    closest = interactable;
                }
            }
        }
        return closest;
    }

}
