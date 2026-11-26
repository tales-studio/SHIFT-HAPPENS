using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "FootstepSurfaceTable", menuName = "Shift Happens/Audio/Footstep Surface Table")]
public class FootstepSurfaceTable : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public FootstepSurface surface;
        public List<TileBase> tiles = new List<TileBase>();
    }

    [SerializeField] private FootstepSurface defaultSurface;
    [SerializeField] private List<Entry> entries = new List<Entry>();

    [NonSerialized] private Dictionary<TileBase, FootstepSurface> lookup;

    public FootstepSurface GetSurface(TileBase tile)
    {
        if (tile == null) return defaultSurface;
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(tile, out FootstepSurface surface) ? surface : defaultSurface;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<TileBase, FootstepSurface>();
        foreach (Entry entry in entries)
        {
            if (entry == null || entry.surface == null) continue;
            foreach (TileBase tile in entry.tiles)
            {
                if (tile == null) continue;
                if (lookup.ContainsKey(tile))
                {
                    Debug.LogWarning($"FootstepSurfaceTable: {tile.name} is listed more than once. The first entry is used.", this);
                    continue;
                }
                lookup.Add(tile, entry.surface);
            }
        }
    }

    private void OnValidate()
    {
        lookup = null;
    }
}
