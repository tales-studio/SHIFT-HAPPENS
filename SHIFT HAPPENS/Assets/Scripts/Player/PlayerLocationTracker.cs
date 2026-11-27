using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerLocationTracker : MonoBehaviour
{
    private const float MovementThresholdSqr = 0.0001f;

    [SerializeField, Min(0.02f)] private float checkInterval = 0.15f;

    private Rigidbody2D body;
    private PlayerTeleporter teleporter;
    private float nextCheckTime;
    private Vector2 lastCheckedPosition;
    private bool hasChecked;

    public LocationInfo Current { get; private set; } = LocationInfo.None;

    public event Action<LocationInfo> LocationChanged;

    private Vector2 Position => body != null ? body.position : (Vector2)transform.position;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        teleporter = GetComponent<PlayerTeleporter>();
    }

    private void OnEnable()
    {
        nextCheckTime = 0f;
        hasChecked = false;

        if (teleporter != null) teleporter.OnTeleported += Refresh;
    }

    private void OnDisable()
    {
        if (teleporter != null) teleporter.OnTeleported -= Refresh;
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime) return;

        if (hasChecked && (Position - lastCheckedPosition).sqrMagnitude < MovementThresholdSqr)
        {
            nextCheckTime = Time.unscaledTime + checkInterval;
            return;
        }

        Refresh();
    }

    public void Refresh()
    {
        nextCheckTime = Time.unscaledTime + checkInterval;
        lastCheckedPosition = Position;
        hasChecked = true;

        LocationInfo location = Current.HasArea
            ? LocationInfo.At(lastCheckedPosition, Current.Floor)
            : LocationInfo.At(lastCheckedPosition);

        if (location == Current) return;

        Current = location;
        LocationChanged?.Invoke(location);
    }
}
