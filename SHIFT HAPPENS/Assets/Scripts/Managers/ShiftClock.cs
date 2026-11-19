using System;
using UnityEngine;

public class ShiftClock : MonoBehaviour
{
    [SerializeField] private int startHour = 22;
    [SerializeField] private int endHour = 6;

    private float elapsedMinutes;
    private int hoursNotified;
    private bool isRunning;
    public int TotalHours => (endHour - startHour + 24) % 24;
    public float TotalMinutes => TotalHours * 60f;
    public int CurrentHour => (startHour + (int)(elapsedMinutes / 60f)) % 24;
    public int CurrentMinute => (int)(elapsedMinutes % 60f);
    public string TimeString => $"{CurrentHour:00}:{CurrentMinute:00}";
    public event Action<int> OnHourChanged;
    public event Action<float> OnTimeAdvanced;
    public event Action OnShiftEnded;

    public void AdvanceTime(float gameMinutes)
    {
        if (!isRunning || gameMinutes <= 0f) return;
        
        elapsedMinutes = Mathf.Min(elapsedMinutes + gameMinutes, TotalMinutes);
        OnTimeAdvanced?.Invoke(gameMinutes);

        int hoursPassed = (int)(elapsedMinutes / 60f);
        while (hoursNotified < hoursPassed)
        {
            hoursNotified++;
            OnHourChanged?.Invoke((startHour + hoursNotified) % 24);
        }

        if (elapsedMinutes >= TotalMinutes)
        {
            isRunning = false;
            OnShiftEnded?.Invoke();
        }
    }

    public void StartClock()
    {
        isRunning = true;
        OnHourChanged?.Invoke(startHour); 
    }
    public void ResetClock()
    {
        elapsedMinutes = 0f;
        hoursNotified = 0;
    }

    public void StopClock() => isRunning = false;
    public void ResumeClock() => isRunning = true;

    [ContextMenu("Debug: Advance 1 hour")]
    private void DebugAdvanceHour() => AdvanceTime(60f);

}
