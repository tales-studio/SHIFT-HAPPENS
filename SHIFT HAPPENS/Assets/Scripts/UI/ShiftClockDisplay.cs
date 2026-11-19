using TMPro;
using UnityEngine;

public class ShiftClockDisplay : MonoBehaviour
{
    [SerializeField] private ShiftClock clock;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private float hourPulse = 0.15f;

    private float pulse;

    private void Start()
    {
        if (clock == null && GameManager.Instance != null) clock = GameManager.Instance.Clock;
        if (clock == null) return;

        clock.OnTimeAdvanced += _ => Refresh();
        clock.OnHourChanged += _ => pulse = 1f;

        Refresh();
    }

    private void Update()
    {
        if (pulse <= 0f) return;
        pulse = Mathf.MoveTowards(pulse, 0f, Time.deltaTime * 3f);
        transform.localScale = Vector3.one * (1f + pulse * hourPulse);
    }

    private void Refresh()
    {
        if (timeText != null) timeText.text = clock.TimeString;
    }
}