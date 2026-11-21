using UnityEngine;

[RequireComponent(typeof(Door))]
public class Locker : MonoBehaviour
{
    [SerializeField] private Collider2D innerFrontWall;

    [SerializeField] private Collider2D outerBlocker;

    [SerializeField] private Collider2D insideZone;

    [SerializeField] private float floorHeight = 1.05f;

    private Door door;
    private bool playerShutInside;

    public bool IsPlayerHidden => !door.IsOpen && playerShutInside;

    private void Awake()
    {
        door = GetComponent<Door>();

        if (insideZone != null && !insideZone.isTrigger)
        {
            Debug.LogWarning($"{name}: the Locker inside zone collider should be a trigger.", this);
        }
    }

    private void OnEnable()
    {
        door.OpenStateChanged += ApplyState;
    }

    private void OnDisable()
    {
        door.OpenStateChanged -= ApplyState;
        if (Player.Instance != null) Player.Instance.SetHidden(false, this);
    }

    private void Start()
    {
        ApplyState(door.IsOpen);
    }

    private void ApplyState(bool open)
    {
        Player player = Player.Instance;
        playerShutInside = !open && IsInside(player);

        if (playerShutInside) PlaceOnFloor(player);

        if (innerFrontWall != null) innerFrontWall.enabled = playerShutInside;
        if (outerBlocker != null) outerBlocker.enabled = !open && !playerShutInside;
        if (player != null) player.SetHidden(playerShutInside, this);
    }

    private bool IsInside(Player player)
    {
        return player != null && insideZone != null && insideZone.OverlapPoint(player.Position);
    }

    private void PlaceOnFloor(Player player)
    {
        float minY = transform.position.y + floorHeight;
        Vector3 position = player.transform.position;
        if (position.y >= minY) return;

        position.y = minY;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = position;
        player.transform.position = position;
    }
}
