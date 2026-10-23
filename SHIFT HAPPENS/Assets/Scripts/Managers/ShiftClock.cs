using System;
using UnityEngine;

public class ShiftClock : MonoBehaviour
{
    [SerializeField] private int startHour = 22;
    [SerializeField] private int endHour = 6;
    [SerializeField] private float realSecondsPerGameHour = 225f;
    [SerializeField] private float debugSpeedMultiplier = 1f;

    public event Action<int> OnHourChanged;

    private int elapsedGameHours;   
    private float elapsedTime;  
    private int hoursAlreadyNotified;
    private bool isRunning;
    public int totalHours => (endHour - startHour + 24) % 24;
    public int currentHour => (startHour + elapsedGameHours) % 24;
    public string TimeString => $"{currentHour:00}";

    private void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime * debugSpeedMultiplier;
        elapsedGameHours = (int) (elapsedTime / realSecondsPerGameHour);

        OnHourChanged?.Invoke((startHour + hoursAlreadyNotified) % 24);
    }

    public void StartClock()
    {
        isRunning = true;
        OnHourChanged?.Invoke(startHour); 
    }
    public void ResetClock()
    {
        elapsedTime = 0;
    }

    public void StopClock() => isRunning = false;
    public void ResumeClock() => isRunning = true;
}
