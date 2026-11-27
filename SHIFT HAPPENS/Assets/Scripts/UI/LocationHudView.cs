using System.Globalization;
using TMPro;
using UnityEngine;

public class LocationHudView : MonoBehaviour
{
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private GameObject background;
    [SerializeField] private FloorCatalog floorCatalog;
    [SerializeField, Min(1f)] private float floorSizePercent = 130f;
    [SerializeField, Min(1f)] private float roomSizePercent = 85f;
    [SerializeField, Min(0f)] private float changeFadeDuration = 0.25f;

    private PlayerLocationTracker tracker;
    private float baseAlpha = 1f;
    private float fadeElapsed = float.MaxValue;

    private void Start()
    {
        if (locationText != null) baseAlpha = locationText.alpha;

        if (Player.Instance != null) tracker = Player.Instance.Location;
        if (tracker == null)
        {
            Show(LocationInfo.None, false);
            return;
        }

        tracker.LocationChanged += HandleLocationChanged;
        Show(tracker.Current, false);
    }

    private void OnDestroy()
    {
        if (tracker != null) tracker.LocationChanged -= HandleLocationChanged;
    }

    private void HandleLocationChanged(LocationInfo location)
    {
        Show(location, true);
    }

    private void Update()
    {
        if (locationText == null || fadeElapsed >= changeFadeDuration) return;

        fadeElapsed += Time.unscaledDeltaTime;
        locationText.alpha = baseAlpha * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(fadeElapsed / changeFadeDuration));
    }

    private void Show(LocationInfo location, bool animate)
    {
        if (locationText == null) return;

        string text = Format(location);
        locationText.text = text;
        if (background != null) background.SetActive(!string.IsNullOrEmpty(text));

        if (animate && changeFadeDuration > 0f)
        {
            fadeElapsed = 0f;
            locationText.alpha = 0f;
            return;
        }

        fadeElapsed = float.MaxValue;
        locationText.alpha = baseAlpha;
    }

    private string Format(LocationInfo location)
    {
        if (!location.HasArea) return string.Empty;

        string floorName = floorCatalog != null ? floorCatalog.GetName(location.Floor) : location.Floor.ToString();
        string roomName = location.RoomName;
        string floorLine = Sized($"{floorName}", floorSizePercent);

        if (string.IsNullOrEmpty(roomName) || roomName == floorName) return floorLine;

        return floorLine + "\n" + Sized($"- {roomName} -", roomSizePercent);
    }

    private static string Sized(string text, float percent)
    {
        return $"<size={percent.ToString("0.##", CultureInfo.InvariantCulture)}%>{text}</size>";
    }
}
