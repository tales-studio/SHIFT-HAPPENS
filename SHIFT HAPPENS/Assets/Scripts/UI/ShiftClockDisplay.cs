using TMPro;
using UnityEngine;

public class ShiftClockDisplay : MonoBehaviour
{
    [SerializeField] private ShiftClock clock;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private float hourPulse = 0.15f;
    [SerializeField, Range(0f, 1f)] private float fastForwardDarken = 0.35f;
    [SerializeField] private float darkenFadeSpeed = 6f;

    private float pulse;
    private float darken;
    private Color baseColor;

    private void Start()
    {
        if (timeText != null) baseColor = timeText.color;

        if (clock == null && GameManager.Instance != null) clock = GameManager.Instance.Clock;
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
        pulse = 1f;
    }

    private void Update()
    {
        UpdateDarken();

        if (pulse <= 0f) return;
        pulse = Mathf.MoveTowards(pulse, 0f, Time.deltaTime * 3f);
        transform.localScale = Vector3.one * (1f + pulse * hourPulse);
    }

    private void UpdateDarken()
    {
        if (timeText == null || clock == null) return;

        float target = clock.IsFastForwarding ? 1f : 0f;
        if (Mathf.Approximately(darken, target)) return;

        darken = Mathf.MoveTowards(darken, target, Time.deltaTime * darkenFadeSpeed);
        Color dark = Color.Lerp(baseColor, Color.black, fastForwardDarken);
        dark.a = baseColor.a;
        timeText.color = Color.Lerp(baseColor, dark, darken);
    }

    private void Refresh()
    {
        if (timeText != null) timeText.text = clock.TimeString;
    }
}
