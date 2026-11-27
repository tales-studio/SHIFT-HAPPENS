using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorCatalog", menuName = "Shift Happens/Locations/Floor Catalog")]
public class FloorCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public BuildingFloor floor;
        public string displayName;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    [NonSerialized] private Dictionary<BuildingFloor, string> lookup;

    public string GetName(BuildingFloor floor)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(floor, out string displayName) ? displayName : floor.ToString();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<BuildingFloor, string>();
        foreach (Entry entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.displayName)) continue;
            if (lookup.ContainsKey(entry.floor))
            {
                Debug.LogWarning($"FloorCatalog: {entry.floor} is listed more than once. The first entry is used.", this);
                continue;
            }
            lookup.Add(entry.floor, entry.displayName);
        }
    }

    private void OnValidate()
    {
        lookup = null;
    }
}
