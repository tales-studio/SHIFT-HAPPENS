using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TilemapSurfaceProvider : MonoBehaviour, IFootstepSurfaceProvider
{
    [SerializeField] private FootstepSurfaceTable table;

    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void OnEnable()
    {
        FootstepSurfaces.Register(this);
    }

    private void OnDisable()
    {
        FootstepSurfaces.Unregister(this);
    }

    public FootstepSurface GetSurfaceAt(Vector3 position)
    {
        if (table == null) return null;
        TileBase tile = tilemap.GetTile(tilemap.WorldToCell(position));
        return table.GetSurface(tile);
    }
}
