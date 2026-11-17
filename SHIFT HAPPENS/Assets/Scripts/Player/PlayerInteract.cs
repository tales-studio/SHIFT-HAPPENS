using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Collider2D interactionArea;

    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private ContactFilter2D filter = new ContactFilter2D().NoFilter();

    public void OnInteract()
    {
        IInteractable closest = GetClosestInteractable();

        if (closest != null)
        {
            closest.Interact();
        }
    }

    private IInteractable GetClosestInteractable()
    {
        IInteractable closest = null;
        float minSqrDistance = float.MaxValue;
        Vector2 position = transform.position;

        interactionArea.Overlap(filter, overlaps);

        foreach (Collider2D overlap in overlaps)
        {
            IInteractable interactable = overlap.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float sqrDistance = (position - (Vector2)interactable.GetTransform().position).sqrMagnitude;

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                closest = interactable;
            }
        }
        return closest;
    }
}
