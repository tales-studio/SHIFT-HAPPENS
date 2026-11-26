using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LocationArea : MonoBehaviour
{
    private static readonly List<LocationArea> areas = new List<LocationArea>();
    private static readonly Dictionary<BuildingFloor, List<LocationArea>> areasByFloor = new Dictionary<BuildingFloor, List<LocationArea>>();

    [SerializeField] private string displayName;
    [SerializeField] private BuildingFloor floor;
    [SerializeField] private Vector2 size = new Vector2(10f, 10f);

    private BuildingFloor registeredFloor;

    public string DisplayName => displayName;
    public BuildingFloor Floor => floor;
    public Rect Bounds => new Rect((Vector2)transform.position - size * 0.5f, size);

    private void OnEnable()
    {
        areas.Add(this);

        registeredFloor = floor;
        if (!areasByFloor.TryGetValue(registeredFloor, out List<LocationArea> floorAreas))
        {
            floorAreas = new List<LocationArea>();
            areasByFloor.Add(registeredFloor, floorAreas);
        }
        floorAreas.Add(this);
    }

    private void OnDisable()
    {
        areas.Remove(this);

        if (areasByFloor.TryGetValue(registeredFloor, out List<LocationArea> floorAreas))
        {
            floorAreas.Remove(this);
        }
    }

    public static string NameAt(Vector2 point, string fallback)
    {
        LocationArea area = At(point);
        return area != null && !string.IsNullOrEmpty(area.displayName) ? area.displayName : fallback;
    }

    public static LocationArea At(Vector2 point) => SmallestAt(areas, point);

    public static LocationArea At(Vector2 point, BuildingFloor likelyFloor)
    {
        if (areasByFloor.TryGetValue(likelyFloor, out List<LocationArea> floorAreas))
        {
            LocationArea area = SmallestAt(floorAreas, point);
            if (area != null) return area;
        }

        return At(point);
    }

    private static LocationArea SmallestAt(List<LocationArea> candidates, Vector2 point)
    {
        LocationArea best = null;
        float bestSize = float.MaxValue;

        foreach (LocationArea area in candidates)
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

        return best;
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
        areasByFloor.Clear();
    }
}
