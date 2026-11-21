using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public class PhoneClock : MonoBehaviour
{
    private TMP_Text label;
    private ShiftClock clock;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (GameManager.Instance != null) clock = GameManager.Instance.Clock;
        if (clock == null) return;

        clock.OnTimeAdvanced += HandleTimeAdvanced;
        clock.OnHourChanged += HandleHourChanged;
        Refresh();
    }

    private void OnDestroy()
    {
        if (clock == null) return;

        clock.OnTimeAdvanced -= HandleTimeAdvanced;
        clock.OnHourChanged -= HandleHourChanged;
    }

    private void HandleTimeAdvanced(float minutes)
    {
        Refresh();
    }

    private void HandleHourChanged(int hour)
    {
        Refresh();
    }

    private void Refresh()
    {
        label.text = clock.TimeString;
    }
}
