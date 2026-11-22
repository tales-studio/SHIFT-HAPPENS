using System;
using UnityEngine;

public enum GamePhase
{
    Menu,
    Normal,
    Blackout,
    Outbreak,
    Ended,
    Tutorial
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private ShiftClock clock;
    [SerializeField] private int blackoutHour = 1;
    [SerializeField] private bool startOnPlay = true;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Menu;
    public bool isPlaying => CurrentPhase == GamePhase.Tutorial || CurrentPhase == GamePhase.Normal || CurrentPhase == GamePhase.Blackout || CurrentPhase == GamePhase.Outbreak;
    public bool IsShiftRunning => CurrentPhase == GamePhase.Normal || CurrentPhase == GamePhase.Blackout || CurrentPhase == GamePhase.Outbreak;
    public bool IsPaused { get; private set; }
    public ShiftResult Result { get; private set; }
    public ShiftClock Clock => clock;

    public event Action<GamePhase> OnPhaseChanged;
    public event Action OnBlackout;
    public event Action<ShiftResult> OnShiftEnded;
    public event Action OnPaused;
    public event Action OnResumed;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ResolveClock();
    }

    private void ResolveClock()
    {
        if (clock != null && clock.gameObject.scene.IsValid()) return;

        clock = FindFirstObjectByType<ShiftClock>();
        if (clock == null)
            Debug.LogError("GameManager: no ShiftClock found in the scene.", this);
    }

    private void Start()
    {
        clock.OnHourChanged += HandleHourChanged;
        clock.OnShiftEnded += HandleShiftEnded;

        if (startOnPlay) StartShift();
    }

    private void OnDestroy()
    {
        if (clock != null)
        {
            clock.OnHourChanged -= HandleHourChanged;
            clock.OnShiftEnded -= HandleShiftEnded;
        }
        if (Instance == this) Instance = null;
    }

    public void StartTutorial()
    {
        if (!CanChangeTo(GamePhase.Tutorial)) return;

        Time.timeScale = 1f;
        if (TaskManager.Instance != null) TaskManager.Instance.ClearTasks();
        clock.ResetClock();
        SetPhase(GamePhase.Tutorial);
    }

    public void StartShift()
    {
        if (!CanChangeTo(GamePhase.Normal)) return;

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

    private void HandleShiftEnded()
    {
        EndShift(ShiftResult.Survived);
    }

    private void TriggerBlackout()
    {
        if (!CanChangeTo(GamePhase.Blackout)) return;

        //clock.StopClock();
        SetPhase(GamePhase.Blackout);
        OnBlackout?.Invoke();
    }

    public void StartOutbreak()
    {
        if (!CanChangeTo(GamePhase.Outbreak)) return;

        SetPhase(GamePhase.Outbreak);
        //clock.ResumeClock();
    }

    public void EndShift(ShiftResult result)
    {
        if (!CanChangeTo(GamePhase.Ended)) return;

        Resume();
        clock.StopClock();
        Result = result;
        SetPhase(GamePhase.Ended);
        OnShiftEnded?.Invoke(result);
    }

    public void Pause()
    {
        if (IsPaused || !isPlaying) return;

        IsPaused = true;
        Time.timeScale = 0f;
        OnPaused?.Invoke();
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        OnResumed?.Invoke();
    }

    private void SetPhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }

    private bool CanChangeTo(GamePhase newPhase)
    {
        if (IsValidTransition(CurrentPhase, newPhase)) return true;

        Debug.LogWarning("GameManager: can't go from " + CurrentPhase + " to " + newPhase + ".", this);
        return false;
    }

    private static bool IsValidTransition(GamePhase from, GamePhase to)
    {
        switch (from)
        {
            case GamePhase.Menu:
                return to == GamePhase.Normal || to == GamePhase.Tutorial;
            case GamePhase.Tutorial:
                return to == GamePhase.Normal;
            case GamePhase.Normal:
                return to == GamePhase.Blackout || to == GamePhase.Ended;
            case GamePhase.Blackout:
                return to == GamePhase.Outbreak || to == GamePhase.Ended;
            case GamePhase.Outbreak:
                return to == GamePhase.Ended;
            default:
                return false;
        }
    }
}
