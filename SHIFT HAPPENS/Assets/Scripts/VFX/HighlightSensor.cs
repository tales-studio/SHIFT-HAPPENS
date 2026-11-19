using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HighlightSensor : MonoBehaviour
{
    private readonly Dictionary<IHighlightable, HashSet<Collider2D>> inRange =
        new Dictionary<IHighlightable, HashSet<Collider2D>>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        IHighlightable highlightable = other.GetComponentInParent<IHighlightable>(true);
        if (highlightable == null) return;

        if (!inRange.TryGetValue(highlightable, out HashSet<Collider2D> colliders))
        {
            colliders = new HashSet<Collider2D>();
            inRange.Add(highlightable, colliders);
        }

        bool wasEmpty = colliders.Count == 0;
        colliders.Add(other);

        if (wasEmpty) highlightable.SetHighlighted(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IHighlightable highlightable = other.GetComponentInParent<IHighlightable>(true);
        if (highlightable == null) return;
        if (!inRange.TryGetValue(highlightable, out HashSet<Collider2D> colliders)) return;
        if (!colliders.Remove(other)) return;

        if (colliders.Count > 0) return;

        inRange.Remove(highlightable);
        highlightable.SetHighlighted(false);
    }

    private void OnDisable()
    {
        foreach (IHighlightable highlightable in inRange.Keys)
        {
            if (highlightable as Object != null) highlightable.SetHighlighted(false);
        }
        inRange.Clear();
    }
}
