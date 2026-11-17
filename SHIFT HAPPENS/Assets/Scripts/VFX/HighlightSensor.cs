using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HighlightSensor : MonoBehaviour
{
    private readonly List<IHighlightable> inRange = new List<IHighlightable>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        IHighlightable highlightable = other.GetComponentInParent<IHighlightable>(true);

        if (highlightable == null || inRange.Contains(highlightable)) return;

        inRange.Add(highlightable);
        highlightable.SetHighlighted(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IHighlightable highlightable = other.GetComponentInParent<IHighlightable>(true);

        if (highlightable == null || !inRange.Remove(highlightable)) return;

        highlightable.SetHighlighted(false);
    }

    private void OnDisable()
    {
        foreach (IHighlightable highlightable in inRange)
        {
            if (highlightable as Object != null) highlightable.SetHighlighted(false);
        }
        inRange.Clear();
    }
}
