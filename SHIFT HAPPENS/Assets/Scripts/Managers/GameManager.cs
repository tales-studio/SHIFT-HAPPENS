using System;
using UnityEngine;
using UnityEngine.Events;

public enum GamePhase
{
    Menu,
    Normal,
    Blackout,
    Outbreak, 
    Ended
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private ShiftClock clock;
    [SerializeField] private int blackoutHour = 1;
    [SerializeField] private bool startOnPlay = true;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Menu;
    public bool isPlaying => CurrentPhase == GamePhase.Normal || CurrentPhase == GamePhase.Blackout || CurrentPhase == GamePhase.Outbreak;
    public ShiftClock Clock => clock;

    public event Action<GamePhase> OnPhaseChanged;
    public event Action OnBlackout;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        clock.OnHourChanged += HandleHourChanged;

        if (startOnPlay) StartShift();
    }

    private void OnDestroy()
    {
        if (clock != null)
        {
            clock.OnHourChanged -= HandleHourChanged;
        }
        if (Instance == this) Instance = null;
    }

    public void StartShift()
    {
        Time.timeScale = 1f;
        if (TaskManager.Instance != null) TaskManager.Instance.ClearTasks();
        clock.ResetClock();
        SetPhase(GamePhase.Normal);
        clock.StartClock();
    }

    private void HandleHourChanged(int hour)
    {
        if (CurrentPhase == GamePhase.Normal && hour == blackoutHour)
            TriggerBlackout();
    }

    private void TriggerBlackout()
    {
        clock.StopClock();
        SetPhase(GamePhase.Blackout);
        OnBlackout?.Invoke();
    }

    public void StartOutbreak()
    {
        if (CurrentPhase != GamePhase.Blackout) return;

        SetPhase(GamePhase.Outbreak);
        clock.ResumeClock();
    }
    private void SetPhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.B) && CurrentPhase == GamePhase.Normal)
            //TriggerBlackout();
    }
}
