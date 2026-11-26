using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerLocationTracker : MonoBehaviour
{
    private const float MovementThresholdSqr = 0.0001f;

    [SerializeField, Min(0.02f)] private float checkInterval = 0.15f;

    private float nextCheckTime;
    private Vector2 lastCheckedPosition;
    private bool hasChecked;

    public LocationInfo Current { get; private set; } = LocationInfo.None;

    public event Action<LocationInfo> LocationChanged;

    private void OnEnable()
    {
        nextCheckTime = 0f;
        hasChecked = false;
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime) return;

        if (hasChecked && ((Vector2)transform.position - lastCheckedPosition).sqrMagnitude < MovementThresholdSqr)
        {
            nextCheckTime = Time.unscaledTime + checkInterval;
            return;
        }

        Refresh();
    }

    public void Refresh()
    {
        nextCheckTime = Time.unscaledTime + checkInterval;
        lastCheckedPosition = transform.position;
        hasChecked = true;

        LocationInfo location = Current.HasArea
            ? LocationInfo.At(lastCheckedPosition, Current.Floor)
            : LocationInfo.At(lastCheckedPosition);

        if (location == Current) return;

        Current = location;
        LocationChanged?.Invoke(location);
    }
}
