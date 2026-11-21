using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LocationArea : MonoBehaviour
{
    private static readonly List<LocationArea> areas = new List<LocationArea>();

    [SerializeField] private string displayName;
    [SerializeField] private Vector2 size = new Vector2(10f, 10f);

    public string DisplayName => displayName;
    public Rect Bounds => new Rect((Vector2)transform.position - size * 0.5f, size);

    private void OnEnable()
    {
        areas.Add(this);
    }

    private void OnDisable()
    {
        areas.Remove(this);
    }

    public static string NameAt(Vector2 point, string fallback)
    {
        LocationArea best = null;
        float bestSize = float.MaxValue;

        foreach (LocationArea area in areas)
        {
            Rect bounds = area.Bounds;
            if (!bounds.Contains(point)) continue;

            float areaSize = bounds.width * bounds.height;
            if (areaSize < bestSize)
            {
                best = area;
                bestSize = areaSize;
            }
        }

        return best != null && !string.IsNullOrEmpty(best.displayName) ? best.displayName : fallback;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.6f);
        Gizmos.DrawWireCube(transform.position, size);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        areas.Clear();
    }
}
